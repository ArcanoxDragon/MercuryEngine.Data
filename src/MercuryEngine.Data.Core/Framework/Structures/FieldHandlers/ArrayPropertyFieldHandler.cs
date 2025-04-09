using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Framework.Fields;
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
	TItem
>(ArrayField<TItem> field, PropertyInfo property, bool activateWhenNull = false) : IFieldHandler
where TItem : IBinaryField
{
	private readonly Func<TOwner, IList<TItem>?> getter = ReflectionUtility.GetGetter<TOwner, IList<TItem>?>(property);

	// Setter is lazy-initialized because it requires List<T> instead of IList<T>.
	// IList<T> properties need to work as long as we don't need to activate null lists.
	private Action<TOwner, List<TItem>?>? setter;

	public uint GetSize(IDataStructure dataStructure)
	{
		if (this.getter((TOwner) dataStructure) is null)
			return 0;

		PrepareForWrite(dataStructure);
		return field.Size;
	}

	public IBinaryField GetField(IDataStructure dataStructure)
		=> field;

	public void Reset(IDataStructure dataStructure)
		=> GetListFromProperty(dataStructure).Clear();

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader)
	{
		field.Read(reader);
		PostProcessRead(dataStructure);
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer)
	{
		PrepareForWrite(dataStructure);
		field.Write(writer);
	}

	public async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		PostProcessRead(dataStructure);
	}

	public Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		PrepareForWrite(dataStructure);
		return field.WriteAsync(writer, cancellationToken);
	}

	private void PrepareForWrite(IDataStructure dataStructure)
	{
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

	private void PostProcessRead(IDataStructure dataStructure)
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