using System.Reflection;
using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a <see cref="IList{T}"/> property with items of type <typeparamref name="T"/> using an
/// <see cref="ArrayField{TItem}"/> with items of the same type.
/// </summary>
public class ArrayPropertyFieldHandler<T>(ArrayField<T> field, object owner, PropertyInfo property, bool activateWhenNull = false) : IFieldHandler
where T : IBinaryField
{
	private readonly Func<object, IList<T>?> getter = ReflectionUtility.GetGetter<IList<T>?>(property);

	// Setter is lazy-initialized because it requires List<T> instead of IList<T>.
	// IList<T> properties need to work as long as we don't need to activate null lists.
	private Action<object, List<T>?>? setter;

	public uint Size
	{
		get
		{
			if (this.getter(owner) is null)
				return 0;

			PrepareForWrite();
			return field.Size;
		}
	}

	public IBinaryField Field => field;

	public void Reset() => GetListFromProperty().Clear();

	public void HandleRead(BinaryReader reader)
	{
		field.Read(reader);
		PostProcessRead();
	}

	public void HandleWrite(BinaryWriter writer)
	{
		PrepareForWrite();
		field.Write(writer);
	}

	public async Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		PostProcessRead();
	}

	public Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		PrepareForWrite();
		return field.WriteAsync(writer, cancellationToken);
	}

	private void PrepareForWrite()
	{
		// Update the field list from the property

		var source = GetListFromProperty();
		var destination = field.Value;

		destination.Clear();

		if (source is List<T> sourceList)
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

	private void PostProcessRead()
	{
		// Update the list stored in the property
		var source = field.Value;
		var destination = GetListFromProperty();

		destination.Clear();

		if (destination is List<T> destinationList)
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

	private IList<T> GetListFromProperty()
	{
		var list = this.getter(owner);

		if (list is null)
		{
			if (!activateWhenNull)
				throw new InvalidOperationException($"Property \"{property.Name}\" on {owner.GetType().FullName} returned null while writing to an array field");

			list = new List<T>();

			this.setter ??= ReflectionUtility.GetSetter<List<T>?>(property);
			this.setter(owner, (List<T>) list);
		}

		return list;
	}
}