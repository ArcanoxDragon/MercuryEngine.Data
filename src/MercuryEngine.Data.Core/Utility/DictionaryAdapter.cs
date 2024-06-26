using System.Collections;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Utility;

public class DictionaryAdapter<TBackingKey, TBackingValue, TAccessKey, TAccessValue> : IDictionary<TAccessKey, TAccessValue>
{
	private readonly IDictionary<TBackingKey, TBackingValue> backingDictionary;
	private readonly Func<TBackingKey, TAccessKey>           translateKey;
	private readonly Func<TBackingValue, TAccessValue>       translateValue;
	private readonly Func<TAccessKey, TBackingKey>           translateKeyReverse;
	private readonly Func<TAccessValue, TBackingValue>       translateValueReverse;

	public DictionaryAdapter(
		IDictionary<TBackingKey, TBackingValue> backingDictionary,
		Func<TBackingKey, TAccessKey> translateKey,
		Func<TBackingValue, TAccessValue> translateValue,
		Func<TAccessKey, TBackingKey> translateKeyReverse,
		Func<TAccessValue, TBackingValue> translateValueReverse)
	{
		this.backingDictionary = backingDictionary;
		this.translateKey = translateKey;
		this.translateValue = translateValue;
		this.translateKeyReverse = translateKeyReverse;
		this.translateValueReverse = translateValueReverse;

		Keys = new KeyCollection(this);
		Values = new ValueCollection(this);
	}

	public int  Count      => this.backingDictionary.Count;
	public bool IsReadOnly => this.backingDictionary.IsReadOnly;

	public ICollection<TAccessKey>   Keys   { get; }
	public ICollection<TAccessValue> Values { get; }

	[MustDisposeResource]
	public IEnumerator<KeyValuePair<TAccessKey, TAccessValue>> GetEnumerator()
		=> this.backingDictionary.Select(TranslatePair).GetEnumerator();

	[MustDisposeResource]
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Add(KeyValuePair<TAccessKey, TAccessValue> item)
		=> this.backingDictionary.Add(TranslatePairReverse(item));

	public void Clear()
		=> this.backingDictionary.Clear();

	public bool Contains(KeyValuePair<TAccessKey, TAccessValue> item)
		=> this.backingDictionary.Contains(TranslatePairReverse(item));

	public void CopyTo(KeyValuePair<TAccessKey, TAccessValue>[] array, int arrayIndex)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length);

		if (arrayIndex + Count >= array.Length)
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");

		var index = arrayIndex;

		foreach (var backingPair in this.backingDictionary)
			array[index++] = TranslatePair(backingPair);
	}

	public bool Remove(KeyValuePair<TAccessKey, TAccessValue> item)
		=> this.backingDictionary.Remove(TranslatePairReverse(item));

	public void Add(TAccessKey key, TAccessValue value)
		=> this.backingDictionary.Add(this.translateKeyReverse(key), this.translateValueReverse(value));

	public bool ContainsKey(TAccessKey key)
		=> this.backingDictionary.ContainsKey(this.translateKeyReverse(key));

	public bool Remove(TAccessKey key)
		=> this.backingDictionary.Remove(this.translateKeyReverse(key));

	public bool TryGetValue(TAccessKey key, [MaybeNullWhen(false)] out TAccessValue value)
	{
		if (!this.backingDictionary.TryGetValue(this.translateKeyReverse(key), out var backingValue))
		{
			value = default;
			return false;
		}

		value = this.translateValue(backingValue);
		return true;
	}

	public TAccessValue this[TAccessKey key]
	{
		get => this.translateValue(this.backingDictionary[this.translateKeyReverse(key)]);
		set => this.backingDictionary[this.translateKeyReverse(key)] = this.translateValueReverse(value);
	}

	private KeyValuePair<TAccessKey, TAccessValue> TranslatePair(KeyValuePair<TBackingKey, TBackingValue> backingPair)
		=> new(this.translateKey(backingPair.Key), this.translateValue(backingPair.Value));

	private KeyValuePair<TBackingKey, TBackingValue> TranslatePairReverse(KeyValuePair<TAccessKey, TAccessValue> backingPair)
		=> new(this.translateKeyReverse(backingPair.Key), this.translateValueReverse(backingPair.Value));

	#region Helper Types

	private sealed class KeyCollection(DictionaryAdapter<TBackingKey, TBackingValue, TAccessKey, TAccessValue> owner) : ICollection<TAccessKey>
	{
		public int  Count      => owner.backingDictionary.Keys.Count;
		public bool IsReadOnly => owner.backingDictionary.Keys.IsReadOnly;

		[MustDisposeResource]
		public IEnumerator<TAccessKey> GetEnumerator()
			=> owner.backingDictionary.Keys.Select(owner.translateKey).GetEnumerator();

		[MustDisposeResource]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(TAccessKey item)
			// Proxied "Add" method will throw. No sense trying to translate the item here.
			=> owner.backingDictionary.Keys.Add(default!);

		public void Clear()
			// The proxied "Clear" method will throw
			=> owner.backingDictionary.Keys.Clear();

		public bool Contains(TAccessKey item)
			=> owner.backingDictionary.Keys.Contains(owner.translateKeyReverse(item));

		public void CopyTo(TAccessKey[] array, int arrayIndex)
		{
			var backingArray = new TBackingKey[array.Length];

			// This will validate bounds, etc.
			owner.backingDictionary.Keys.CopyTo(backingArray, arrayIndex);

			for (var i = arrayIndex; i < array.Length; i++)
				array[i] = owner.translateKey(backingArray[i]);
		}

		public bool Remove(TAccessKey item)
			// Proxied "Remove" method will throw. No sense trying to translate the item here.
			=> owner.backingDictionary.Keys.Remove(default!);
	}

	private sealed class ValueCollection(DictionaryAdapter<TBackingKey, TBackingValue, TAccessKey, TAccessValue> owner) : ICollection<TAccessValue>
	{
		public int  Count      => owner.backingDictionary.Values.Count;
		public bool IsReadOnly => owner.backingDictionary.Values.IsReadOnly;

		[MustDisposeResource]
		public IEnumerator<TAccessValue> GetEnumerator()
			=> owner.backingDictionary.Values.Select(owner.translateValue).GetEnumerator();

		[MustDisposeResource]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(TAccessValue item)
			// Proxied "Add" method will throw. No sense trying to translate the item here.
			=> owner.backingDictionary.Values.Add(default!);

		public void Clear()
			// The proxied "Clear" method will throw
			=> owner.backingDictionary.Values.Clear();

		public bool Contains(TAccessValue item)
			=> owner.backingDictionary.Values.Contains(owner.translateValueReverse(item));

		public void CopyTo(TAccessValue[] array, int arrayIndex)
		{
			var backingArray = new TBackingValue[array.Length];

			// This will validate bounds, etc.
			owner.backingDictionary.Values.CopyTo(backingArray, arrayIndex);

			for (var i = arrayIndex; i < array.Length; i++)
				array[i] = owner.translateValue(backingArray[i]);
		}

		public bool Remove(TAccessValue item)
			// Proxied "Remove" method will throw. No sense trying to translate the item here.
			=> owner.backingDictionary.Values.Remove(default!);
	}

	#endregion
}