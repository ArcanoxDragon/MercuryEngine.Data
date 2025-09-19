using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a <see cref="IDictionary{TKey,TValue}"/> property with keys of type <typeparamref name="TKey"/>
/// and values of type <typeparamref name="TValue"/> using a <see cref="DictionaryField{TKey,TValue}"/> with keys and values
/// of the same types.
/// </summary>
public class DictionaryPropertyFieldHandler<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TOwner,
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TKey,
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TValue
>(
	Func<TOwner, DictionaryField<TKey, TValue>> fieldFactory,
	PropertyInfo property,
	bool activateWhenNull = false
) : FieldHandlerWithBackingField<TOwner, DictionaryField<TKey, TValue>>(fieldFactory)
where TOwner : IDataStructure
where TKey : IBinaryField
where TValue : IBinaryField
{
	private readonly Func<TOwner, IDictionary<TKey, TValue>?> getter = ReflectionUtility.GetGetter<TOwner, IDictionary<TKey, TValue>?>(property);

	// Setter is lazy-initialized because it requires Dictionary<TKey, TValue> instead of IDictionary<TKey, TValue>.
	// IDictionary<TKey, TValue> properties need to work as long as we don't need to activate null lists.
	private Action<TOwner, Dictionary<TKey, TValue>?>? setter;

	public override uint GetSize(IDataStructure dataStructure)
	{
		if (this.getter((TOwner) dataStructure) is null)
			return 0;

		PrepareForWrite(dataStructure, out var field);
		return field.Size;
	}

	public override void Reset(IDataStructure dataStructure)
		=> GetDictionaryFromProperty(dataStructure).Clear();

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

	private void PrepareForWrite(IDataStructure dataStructure, out DictionaryField<TKey, TValue> field)
	{
		field = GetField(dataStructure);

		// Update the field dictionary from the property
		var source = GetDictionaryFromProperty(dataStructure);

		field.Value.Clear();

		foreach (var (key, value) in source)
			field.Value.Add(key, value);
	}

	private void PostProcessRead(IDataStructure dataStructure, DictionaryField<TKey, TValue> field)
	{
		// Update the dictionary stored in the property
		var destination = GetDictionaryFromProperty(dataStructure);

		destination.Clear();

		foreach (var (key, value) in field.Value)
			destination.Add(key, value);
	}

	private IDictionary<TKey, TValue> GetDictionaryFromProperty(IDataStructure dataStructure)
	{
		var dictionary = this.getter((TOwner) dataStructure);

		if (dictionary is null)
		{
			if (!activateWhenNull)
				throw new InvalidOperationException($"Property \"{property.Name}\" on {dataStructure.GetType().FullName} returned null while writing to a dictionary field");

			dictionary = new Dictionary<TKey, TValue>();

			this.setter ??= ReflectionUtility.GetSetter<TOwner, Dictionary<TKey, TValue>?>(property);
			this.setter((TOwner) dataStructure, (Dictionary<TKey, TValue>) dictionary);
		}

		return dictionary;
	}
}