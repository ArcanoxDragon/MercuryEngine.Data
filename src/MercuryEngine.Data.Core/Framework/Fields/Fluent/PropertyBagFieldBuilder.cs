using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.Fields.Fluent;

[PublicAPI]
public sealed class PropertyBagFieldBuilder
{
	internal Dictionary<string, Func<IBinaryField>> Fields { get; init; } = [];

	#region Property Fields

	public PropertyBagFieldBuilder String(string propertyName)
		=> AddField<TerminatedStringField>(propertyName);

	public PropertyBagFieldBuilder Boolean(string propertyName)
		=> AddField<BooleanField>(propertyName);

	public PropertyBagFieldBuilder Byte(string propertyName)
		=> AddField<ByteField>(propertyName);

	public PropertyBagFieldBuilder Char(string propertyName)
		=> AddField<CharField>(propertyName);

	public PropertyBagFieldBuilder Int16(string propertyName)
		=> AddField<Int16Field>(propertyName);

	public PropertyBagFieldBuilder UInt16(string propertyName)
		=> AddField<UInt16Field>(propertyName);

	public PropertyBagFieldBuilder Int32(string propertyName)
		=> AddField<Int32Field>(propertyName);

	public PropertyBagFieldBuilder UInt32(string propertyName)
		=> AddField<UInt32Field>(propertyName);

	public PropertyBagFieldBuilder Int64(string propertyName)
		=> AddField<Int64Field>(propertyName);

	public PropertyBagFieldBuilder UInt64(string propertyName)
		=> AddField<UInt64Field>(propertyName);

	public PropertyBagFieldBuilder Float(string propertyName)
		=> AddField<FloatField>(propertyName);

	public PropertyBagFieldBuilder Double(string propertyName)
		=> AddField<DoubleField>(propertyName);

	public PropertyBagFieldBuilder Decimal(string propertyName)
		=> AddField<DecimalField>(propertyName);

	public PropertyBagFieldBuilder Enum<TEnum>(string propertyName)
	where TEnum : struct, Enum
		=> AddField<EnumField<TEnum>>(propertyName);

	#endregion

	#region Collection Properties

	public PropertyBagFieldBuilder Array<TItem>(string propertyName)
	where TItem : class, IBinaryField, new()
		=> Array(propertyName, () => new TItem());

	public PropertyBagFieldBuilder Array<TItem>(string propertyName, Func<TItem> itemFactory)
	where TItem : class, IBinaryField
		=> AddField(propertyName, () => new ArrayField<TItem>(itemFactory));

	public PropertyBagFieldBuilder Dictionary<TKey, TValue>(string propertyName)
	where TKey : class, IBinaryField, new()
	where TValue : class, IBinaryField, new()
		=> Dictionary(propertyName, () => new TKey(), () => new TValue());

	public PropertyBagFieldBuilder Dictionary<TKey, TValue>(string propertyName, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryField
	where TValue : class, IBinaryField
		=> AddField(propertyName, () => new DictionaryField<TKey, TValue>(keyFactory, valueFactory));

	#endregion

	public PropertyBagFieldBuilder AddField<TField>(string propertyName)
	where TField : IBinaryField, new()
		=> AddField(propertyName, () => new TField());

	public PropertyBagFieldBuilder AddField(string propertyName, Func<IBinaryField> fieldFactory)
	{
		if (!Fields.TryAdd(propertyName, fieldFactory))
			throw new InvalidOperationException($"A property with the key \"{propertyName}\" has already been defined");

		return this;
	}
}