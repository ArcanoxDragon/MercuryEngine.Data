using System.Collections;
using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Utility;

public class ListAdapter<TBackingValue, TAccessValue>(
	IList<TBackingValue> backingList,
	Func<TBackingValue, TAccessValue> translateValue,
	Func<TAccessValue, TBackingValue> translateValueReverse
) : IList<TAccessValue>
{
	private readonly IList<TBackingValue>              backingList           = backingList;
	private readonly Func<TBackingValue, TAccessValue> translateValue        = translateValue;
	private readonly Func<TAccessValue, TBackingValue> translateValueReverse = translateValueReverse;

	#region IList

	public int IndexOf(TAccessValue item)
		=> this.backingList.IndexOf(this.translateValueReverse(item));

	public void Insert(int index, TAccessValue item)
		=> this.backingList.Insert(index, this.translateValueReverse(item));

	public void RemoveAt(int index)
		=> this.backingList.RemoveAt(index);

	public TAccessValue this[int index]
	{
		get => this.translateValue(this.backingList[index]);
		set => this.backingList[index] = this.translateValueReverse(value);
	}

	#endregion

	#region ICollection

	public int  Count      => this.backingList.Count;
	public bool IsReadOnly => this.backingList.IsReadOnly;

	public void Add(TAccessValue item)
		=> this.backingList.Add(this.translateValueReverse(item));

	public void Clear()
		=> this.backingList.Clear();

	public bool Contains(TAccessValue item)
		=> this.backingList.Contains(this.translateValueReverse(item));

	public bool Remove(TAccessValue item)
		=> this.backingList.Remove(this.translateValueReverse(item));

	public void CopyTo(TAccessValue[] array, int arrayIndex)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length);

		if (array.Length - arrayIndex < Count)
			throw new ArgumentException("Not enough room in the target array");

		var index = arrayIndex;

		foreach (var backingPair in this.backingList)
			array[index++] = this.translateValue(backingPair);
	}

	#endregion

	#region IEnumerable

	[MustDisposeResource]
	public IEnumerator<TAccessValue> GetEnumerator()
		=> this.backingList.Select(this.translateValue).GetEnumerator();

	[MustDisposeResource]
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}