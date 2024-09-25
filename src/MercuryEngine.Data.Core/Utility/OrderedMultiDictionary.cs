using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Extensions;

namespace MercuryEngine.Data.Core.Utility;

public class OrderedMultiDictionary<TKey, TValue> : IDictionary<TKey, TValue>
where TKey : notnull
where TValue : notnull
{
	private readonly Dictionary<TKey, SortedSet<int>> indices;
	private readonly List<KeyValuePair<TKey, TValue>> values = [];

	private readonly KeyCollection   keyCollection;
	private readonly ValueCollection valueCollection;

	public OrderedMultiDictionary(DuplicateKeyHandlingMode duplicateKeyHandlingMode)
		: this(duplicateKeyHandlingMode, EqualityComparer<TKey>.Default) { }

	public OrderedMultiDictionary(IEqualityComparer<TKey> keyComparer)
		: this(DuplicateKeyHandlingMode.HighestIndexTakesPriority, keyComparer) { }

	public OrderedMultiDictionary()
		: this(EqualityComparer<TKey>.Default) { }

	public OrderedMultiDictionary(DuplicateKeyHandlingMode duplicateKeyHandlingMode,
								  IEqualityComparer<TKey> keyComparer)
	{
		this.indices = new Dictionary<TKey, SortedSet<int>>(keyComparer);
		this.keyCollection = new KeyCollection(this);
		this.valueCollection = new ValueCollection(this);

		DuplicateKeyHandlingMode = duplicateKeyHandlingMode;
	}

	/// <summary>
	/// Gets or sets the strategy used by the multi-dictionary for handling multiple values for
	/// a given key when using interfaces that operate on one single item.
	/// </summary>
	public DuplicateKeyHandlingMode DuplicateKeyHandlingMode { get; set; }

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
	public IReadOnlyCollection<TKey> Keys => this.keyCollection;

	/// <summary>
	/// Gets a read-only collection of all values in the dictionary. The order in which the values are returned
	/// is identical to the order in which the values are stored in the dictionary.
	/// </summary>
	public IReadOnlyCollection<TValue> Values => this.valueCollection;

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
	public bool RemoveAll(TKey key)
	{
		if (!this.indices.TryGetValue(key, out var indices))
			return false;

		var removed = false;

		while (indices.Count > 0)
		{
			// Always removing the max index will be faster, as there will be potentially fewer indices
			// that need to be shifted down by the time we get to lower indices.
			RemoveIndex(indices.Max);
			removed = true;
		}

		// Also remove the key from the indices dictionary so that it does not appear in the Keys collection
		// (this should be done AFTER removing the individual values!)
		this.indices.Remove(key);
		return removed;
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

	#region IDictionary<TKey, TValue>

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.indices.Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => this.valueCollection;

	bool IDictionary<TKey, TValue>.Remove(TKey key)
		=> RemoveAll(key);

	#endregion

	#region ICollection<KeyValuePair<TKey, TValue>>

	public bool IsReadOnly => false;

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		=> Add(item.Key, item.Value);

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		=> this.values.Exists(i => Equals(i.Key, item.Key) && Equals(i.Value, item.Value));

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(index, array.Length);

		if (array.Length - index < this.values.Count)
			throw new ArgumentException("Not enough room in the target array");

		foreach (var (i, pair) in this.values.Pairs())
			array[index + i] = pair;
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		var startIndex = 0;
		var removed = false;

		while (this.values.FindIndex(startIndex, TestItem) is var foundIndex and >= 0)
		{
			RemoveIndex(foundIndex);
			startIndex = foundIndex; // We just removed this index, so we should include the new item at that index in the next search
			removed = true;
		}

		return removed;

		bool TestItem(KeyValuePair<TKey, TValue> candidate)
			=> Equals(candidate.Key, item.Key) && Equals(candidate.Value, item.Value);
	}

	#endregion

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

	private sealed class ValueCollection(OrderedMultiDictionary<TKey, TValue> owner) : ICollection<TValue>, IReadOnlyCollection<TValue>
	{
		public int Count => owner.values.Count;

		public IEnumerator<TValue> GetEnumerator()
			=> owner.values.Select(pair => pair.Value).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		#region ICollection (for IDictionary contract)

		bool ICollection<TValue>.IsReadOnly => true;

		void ICollection<TValue>.Add(TValue item)
			=> throw ReadOnlyException();

		void ICollection<TValue>.Clear()
			=> throw ReadOnlyException();

		bool ICollection<TValue>.Contains(TValue item)
			=> owner.values.Exists(pair => Equals(pair.Value, item));

		void ICollection<TValue>.CopyTo(TValue[] array, int index)
		{
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(index, array.Length);

			if (array.Length - index < owner.values.Count)
				throw new ArgumentException("Not enough room in the target array");

			foreach (var (i, (_, value)) in owner.values.Pairs())
				array[index + i] = value;
		}

		bool ICollection<TValue>.Remove(TValue item)
			=> throw ReadOnlyException();

		private static NotSupportedException ReadOnlyException()
			=> new("Mutating a key collection derived from a dictionary is not allowed.");

		#endregion
	}

	#endregion
}