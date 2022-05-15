using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.DataTypes;

[PublicAPI]
public class DictionaryDataType<TKey, TValue> : ArrayDataType<KeyValuePairDataType<TKey, TValue>>
where TKey : IBinaryDataType
where TValue : IBinaryDataType
{
	/// <summary>
	/// Constructor that uses reflection to construct data types
	/// TODO: Find alternative way to do this
	/// </summary>
	public DictionaryDataType() : this(Activator.CreateInstance<TKey>, Activator.CreateInstance<TValue>) { }

	public DictionaryDataType(Func<TKey> keyFactory, Func<TValue> valueFactory)
		: this(() => new KeyValuePairDataType<TKey, TValue>(keyFactory(), valueFactory())) { }

	public DictionaryDataType(Func<KeyValuePairDataType<TKey, TValue>> entryFactory)
		: base(entryFactory) { }

	public DictionaryDataType(Func<KeyValuePairDataType<TKey, TValue>> entryFactory, List<KeyValuePairDataType<TKey, TValue>> initialValue)
		: base(entryFactory, initialValue) { }
}