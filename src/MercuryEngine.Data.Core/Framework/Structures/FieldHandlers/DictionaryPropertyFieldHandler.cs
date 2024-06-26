using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a <see cref="IDictionary{TKey,TValue}"/> property with keys of type <typeparamref name="TKey"/>
/// and values of type <typeparamref name="TValue"/> using a <see cref="DictionaryField{TKey,TValue}"/> with keys and values
/// of the same types.
/// </summary>
public class DictionaryPropertyFieldHandler<TKey, TValue>(DictionaryField<TKey, TValue> field, object owner, PropertyInfo property, bool activateWhenNull = false) : IFieldHandler
where TKey : IBinaryField
where TValue : IBinaryField
{
	public uint Size
	{
		get
		{
			if (property.GetValue(owner) is null)
				return 0;

			PrepareForWrite();
			return field.Size;
		}
	}

	public IBinaryField Field => field;

	public void Reset() => GetDictionaryFromProperty().Clear();

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
		// Update the field dictionary from the property
		var source = GetDictionaryFromProperty();

		field.Value.Clear();

		foreach (var (key, value) in source)
			field.Value.Add(new KeyValuePairField<TKey, TValue>(key, value));
	}

	private void PostProcessRead()
	{
		// Update the dictionary stored in the property
		var destination = GetDictionaryFromProperty();

		destination.Clear();

		foreach (var (key, value) in field.Value)
			destination.Add(key, value);
	}

	private IDictionary<TKey, TValue> GetDictionaryFromProperty()
	{
		var value = property.GetValue(owner);

		if (value is null)
		{
			if (!activateWhenNull)
				throw new InvalidOperationException($"Property \"{property.Name}\" on {owner.GetType().FullName} returned null while writing to a dictionary field");

			value = new Dictionary<TKey, TValue>();
			property.SetValue(owner, value);
		}

		if (value is not IDictionary<TKey, TValue> dictionary)
			throw new InvalidOperationException($"Property \"{property.Name}\" on {owner.GetType().FullName} returned a value of type " +
												$"\"{value.GetType().FullName}\" when \"{typeof(IDictionary<TKey, TValue>).FullName}\" was expected");

		return dictionary;
	}
}