using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Fields.PropertyBags;

internal class PropertyBagDictionaryAdapter<TKey, TValue>(
	IPropertyBagField propertyBag,
	string propertyName
) : IDictionaryProperty<TKey, TValue>
where TKey : IBinaryField
where TValue : IBinaryField
{
	private static readonly IDictionary<TKey, TValue> Empty = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());

	private IDictionary<TKey, TValue> Dictionary
		=> (IDictionary<TKey, TValue>?) DictionaryField?.Value ?? Empty;

	private DictionaryField<TKey, TValue>? DictionaryField
	{
		get
		{
			var field = propertyBag.Get(propertyName);

			if (field is null)
				return null;

			if (field is not DictionaryField<TKey, TValue> dictionaryField)
				throw new InvalidOperationException($"Property \"{propertyName}\" is not a Dictionary<{nameof(TKey)}, {nameof(TValue)}> field");

			return dictionaryField;
		}
	}

	public void ClearProperty()
		=> propertyBag.ClearProperty(propertyName);

	#region IDictionary

	public TValue this[TKey key]
	{
		get => Dictionary[key];
		set
		{
			EnsureFieldPresent();
			Dictionary[key] = value;
		}
	}

	public ICollection<TKey>   Keys   => Dictionary.Keys;
	public ICollection<TValue> Values => Dictionary.Values;

	public void Add(TKey key, TValue value)
	{
		EnsureFieldPresent();
		Dictionary.Add(key, value);
	}

	public bool ContainsKey(TKey key)
		=> Dictionary.ContainsKey(key);

	public bool Remove(TKey key)
		// Don't want the empty ReadOnlyDictionary to throw if the field is absent
		=> DictionaryField is not null && Dictionary.Remove(key);

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
		=> Dictionary.TryGetValue(key, out value);

	#endregion

	#region ICollection

	public int  Count      => Dictionary.Count;
	public bool IsReadOnly => false;

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		EnsureFieldPresent();
		Dictionary.Add(item);
	}

	public void Clear()
		// Don't want the empty ReadOnlyDictionary to throw if the field is absent
		=> DictionaryField?.Value.Clear();

	public bool Contains(KeyValuePair<TKey, TValue> item)
		=> Dictionary.Contains(item);

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		=> Dictionary.CopyTo(array, index);

	public bool Remove(KeyValuePair<TKey, TValue> item)
		// Don't want the empty ReadOnlyDictionary to throw if the field is absent
		=> DictionaryField is not null && Dictionary.Remove(item);

	#endregion

	#region IEnumerable

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		=> Dictionary.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion

	private void EnsureFieldPresent()
	{
		if (DictionaryField is null)
			propertyBag.SetValue(propertyName, new OrderedMultiDictionary<TKey, TValue>());
	}
}