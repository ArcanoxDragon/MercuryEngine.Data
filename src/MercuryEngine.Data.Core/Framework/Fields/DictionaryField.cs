using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class DictionaryField<TKey, TValue> : ArrayField<KeyValuePairField<TKey, TValue>>
where TKey : IBinaryField
where TValue : IBinaryField
{
	public DictionaryField()
		: this(ReflectionUtility.CreateFactoryFromDefaultConstructor<TKey>(), ReflectionUtility.CreateFactoryFromDefaultConstructor<TValue>()) { }

	public DictionaryField(Func<TKey> keyFactory, Func<TValue> valueFactory)
		: this(() => new KeyValuePairField<TKey, TValue>(keyFactory(), valueFactory())) { }

	public DictionaryField(Func<KeyValuePairField<TKey, TValue>> itemFactory)
		: base(itemFactory) { }

	public DictionaryField(Func<KeyValuePairField<TKey, TValue>> itemFactory, List<KeyValuePairField<TKey, TValue>> initialValue)
		: base(itemFactory, initialValue) { }

	protected override string MappingDescription => $"Dictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

	protected override string GetEntryMappingDescription(int index, KeyValuePairField<TKey, TValue> entry)
		=> $"key: {entry.Key}";
}

[PublicAPI]
public static class BinaryDictionary
{
	public static DictionaryField<TKey, TValue> Create<TKey, TValue>()
	where TKey : IBinaryField, new()
	where TValue : IBinaryField, new()
		=> new(() => new KeyValuePairField<TKey, TValue>(new TKey(), new TValue()));
}