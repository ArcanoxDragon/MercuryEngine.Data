﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.Core.Utility;

public class OrderedMultiDictionary<TKey, TValue>(
	DuplicateKeyHandlingMode duplicateKeyHandlingMode,
	IEqualityComparer<TKey> keyComparer
) : IEnumerable<KeyValuePair<TKey, TValue>>
where TKey : notnull
where TValue : notnull
{
	private readonly Dictionary<TKey, SortedSet<int>> indices = new(keyComparer);
	private readonly List<KeyValuePair<TKey, TValue>> values  = [];

	public OrderedMultiDictionary(DuplicateKeyHandlingMode duplicateKeyHandlingMode)
		: this(duplicateKeyHandlingMode, EqualityComparer<TKey>.Default) { }

	public OrderedMultiDictionary(IEqualityComparer<TKey> keyComparer)
		: this(DuplicateKeyHandlingMode.HighestIndexTakesPriority, keyComparer) { }

	public OrderedMultiDictionary()
		: this(EqualityComparer<TKey>.Default) { }

	/// <summary>
	/// Gets or sets the strategy used by the multi-dictionary for handling multiple values for
	/// a given key when using interfaces that operate on one single item.
	/// </summary>
	public DuplicateKeyHandlingMode DuplicateKeyHandlingMode { get; set; } = duplicateKeyHandlingMode;

	/// <summary>
	/// Gets or sets the value for the provided <typeparamref name="TKey"/> in the dictionary.
	/// When getting values, if there is more than one stored value, only the highest-index value is returned.
	/// When setting values, all existing values for the given key are removed except for the highest-index value.
	/// </summary>
	public TValue this[TKey key]
	{
		get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
		set => Set(key, value);
	}

	/// <summary>
	/// Gets the number of key-value pairs stored in the dictionary.
	/// </summary>
	public int Count => this.values.Count;

	/// <summary>
	/// Gets a read-only collection of all unique keys in the dictionary. The order in which the keys are returned
	/// is not defined, and may not match the order of the stored key-value pairs.
	/// </summary>
	public IReadOnlyCollection<TKey> Keys => new KeyCollection(this);

	/// <summary>
	/// Gets a read-only collection of all values in the dictionary. The order in which the values are returned
	/// is identical to the order in which the values are stored in the dictionary.
	/// </summary>
	public IReadOnlyCollection<TValue> Values => new ValueCollection(this);

	/// <summary>
	/// Adds a new <paramref name="value"/> to the dictionary for the provided <paramref name="key"/>.
	/// The value is added to the end of the value collection.
	/// </summary>
	public void Add(TKey key, TValue value)
	{
		if (DuplicateKeyHandlingMode == DuplicateKeyHandlingMode.PreventDuplicateKeys)
			RemoveAll(key);

		var newIndex = this.values.Count;
		var indices = GetIndexSet(key);

		this.values.Add(new KeyValuePair<TKey, TValue>(key, value));
		indices.Add(newIndex);
	}

	/// <summary>
	/// Removes all but the highest-index value for the provided <paramref name="key"/> (if any exist),
	/// and replaces the highest-index value with the provided <paramref name="value"/>.
	///
	/// If the provided <paramref name="key"/> does not exist in the dictionary, it is added to the end
	/// of the value collection.
	/// </summary>
	public void Set(TKey key, TValue value)
	{
		if (!ContainsKey(key))
		{
			// Just add the value
			Add(key, value);
			return;
		}

		var indices = GetIndexSet(key);

		while (indices.Count > 1)
		{
			if (DuplicateKeyHandlingMode == DuplicateKeyHandlingMode.HighestIndexTakesPriority)
				// Remove all but the highest index in the set
				RemoveIndex(indices.Min);
			else
				// Remove all but the lowest index in the set
				RemoveIndex(indices.Max);
		}

		// The last remaining value in the set is the new index in the value collection where we will store the new value
		this.values[indices.Max] = new KeyValuePair<TKey, TValue>(key, value);
	}

	/// <summary>
	/// Removes all values for the provided <paramref name="key"/> from the dictionary.
	/// </summary>
	public void RemoveAll(TKey key)
	{
		if (!this.indices.TryGetValue(key, out var indices))
			return;

		while (indices.Count > 0)
			// Always removing the max index will be faster, as there will be potentially fewer indices
			// that need to be shifted down by the time we get to lower indices.
			RemoveIndex(indices.Max);

		// Also remove the key from the indices dictionary so that it does not appear in the Keys collection
		// (this should be done AFTER removing the individual values!)
		this.indices.Remove(key);
	}

	/// <summary>
	/// Removes all values from the dictionary.
	/// </summary>
	public void Clear()
	{
		this.values.Clear();
		this.indices.Clear();
	}

	/// <summary>
	/// Returns whether or not the list contains any values for the provided <paramref name="key"/>.
	/// </summary>
	public bool ContainsKey(TKey key)
		=> this.indices.TryGetValue(key, out var indices) && indices.Count > 0;

	/// <summary>
	/// Tries to retrieve the highest-index value for the provided <paramref name="key"/>.
	/// If there is at least one value for the <paramref name="key"/>, this method returns <see langword="true" />.
	/// If no values are found, this method returns <see langword="false"/>.
	/// </summary>
	public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		if (!this.indices.TryGetValue(key, out var indices) || indices.Count == 0)
		{
			value = default;
			return false;
		}

		var index = DuplicateKeyHandlingMode == DuplicateKeyHandlingMode.HighestIndexTakesPriority
			? indices.Max
			: indices.Min;

		value = this.values[index].Value;
		return true;
	}

	/// <summary>
	/// Returns a sequence of all values in the dictionary for the provided <paramref name="key"/>.
	/// If no values are found, an empty sequence is returned.
	/// </summary>
	public IEnumerable<TValue> GetAllValues(TKey key)
	{
		if (!this.indices.TryGetValue(key, out var indices))
			yield break;

		foreach (var index in indices)
			yield return this.values[index].Value;
	}

	private void RemoveIndex(int index)
	{
		// We have to iterate through all indices and remove the reference to the index that is being removed.
		// We also have to shift any indices higher than the one being removed down by one.

		foreach (var (_, indices) in this.indices)
		{
			var indicesToKeep = indices.Where(i => i != index).Select(i => i > index ? i - 1 : i).ToArray();

			indices.Clear();
			indices.UnionWith(indicesToKeep);
		}

		this.values.RemoveAt(index);
	}

	private SortedSet<int> GetIndexSet(TKey key)
	{
		if (!this.indices.TryGetValue(key, out var indices))
		{
			indices = [];
			this.indices.Add(key, indices);
		}

		return indices;
	}

	#region IEnumerable<KVP>

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		=> this.values.GetEnumerator();

	#endregion

	#region Helper Types

	private sealed class KeyCollection(OrderedMultiDictionary<TKey, TValue> owner) : IReadOnlyCollection<TKey>
	{
		public int Count => owner.indices.Keys.Count;

		public IEnumerator<TKey> GetEnumerator()
			=> owner.indices.Keys.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}

	private sealed class ValueCollection(OrderedMultiDictionary<TKey, TValue> owner) : IReadOnlyCollection<TValue>
	{
		public int Count => owner.values.Count;

		public IEnumerator<TValue> GetEnumerator()
			=> owner.values.Select(pair => pair.Value).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}

	#endregion
}