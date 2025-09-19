using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a <see cref="IList{T}"/> property with items of type <typeparamref name="TItem"/> using an
/// <see cref="ArrayField{TItem}"/> with items of the same type.
/// </summary>
public class ArrayPropertyFieldHandler<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TOwner,
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TItem
>(Func<TOwner, ArrayField<TItem>> fieldFactory, PropertyInfo property, bool activateWhenNull = false)
	: FieldHandlerWithBackingField<TOwner, ArrayField<TItem>>(fieldFactory)
where TOwner : IDataStructure
where TItem : IBinaryField
{
	private readonly Func<TOwner, IList<TItem>?> getter = ReflectionUtility.GetGetter<TOwner, IList<TItem>?>(property);

	// Setter is lazy-initialized because it requires List<T> instead of IList<T>.
	// IList<T> properties need to work as long as we don't need to activate null lists.
	private Action<TOwner, List<TItem>?>? setter;

	public override uint GetSize(IDataStructure dataStructure)
	{
		if (this.getter((TOwner) dataStructure) is null)
			return 0;

		PrepareForWrite(dataStructure, out var field);
		return field.Size;
	}

	public override void Reset(IDataStructure dataStructure)
		=> GetListFromProperty(dataStructure).Clear();

	public override void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		var field = GetField(dataStructure);

		field.Read(reader, context);
		PostProcessRead(dataStructure, field);
	}

	public override void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
	{
		PrepareForWrite(dataStructure, out var field);
		field.Write(writer, context);
	}

	public override async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		var field = GetField(dataStructure);

		await field.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		PostProcessRead(dataStructure, field);
	}

	public override Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
	{
		PrepareForWrite(dataStructure, out var field);
		return field.WriteAsync(writer, context, cancellationToken: cancellationToken);
	}

	private void PrepareForWrite(IDataStructure dataStructure, out ArrayField<TItem> field)
	{
		field = GetField(dataStructure);

		// Update the field list from the property

		var source = GetListFromProperty(dataStructure);
		var destination = field.Value;

		destination.Clear();

		if (source is List<TItem> sourceList)
		{
			// Fast: copy using span
			var sourceSpan = CollectionsMarshal.AsSpan(sourceList);

			destination.AddRange(sourceSpan);
		}
		else
		{
			// Slower: copy using enumeration
			destination.AddRange(source);
		}
	}

	private void PostProcessRead(IDataStructure dataStructure, ArrayField<TItem> field)
	{
		// Update the list stored in the property
		var source = field.Value;
		var destination = GetListFromProperty(dataStructure);

		destination.Clear();

		if (destination is List<TItem> destinationList)
		{
			// Fast: copy using span
			var sourceSpan = CollectionsMarshal.AsSpan(source);

			destinationList.AddRange(sourceSpan);
		}
		else
		{
			// Slower: copy using enumeration
			foreach (var item in source)
				destination.Add(item);
		}
	}

	private IList<TItem> GetListFromProperty(IDataStructure dataStructure)
	{
		var list = this.getter((TOwner) dataStructure);

		if (list is null)
		{
			if (!activateWhenNull)
				throw new InvalidOperationException($"Property \"{property.Name}\" on {dataStructure.GetType().FullName} returned null while writing to an array field");

			list = new List<TItem>();

			this.setter ??= ReflectionUtility.GetSetter<TOwner, List<TItem>?>(property);
			this.setter((TOwner) dataStructure, (List<TItem>) list);
		}

		return list;
	}
}