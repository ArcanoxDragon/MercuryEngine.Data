using System.Collections;
using System.Collections.ObjectModel;

namespace MercuryEngine.Data.Core.Framework.Fields.PropertyBags;

internal class PropertyBagListAdapter<T>(
	IPropertyBagField propertyBag,
	string propertyName
) : IListProperty<T>
where T : IBinaryField
{
	private static readonly IList<T> Empty = new ReadOnlyCollection<T>([]);

	private IList<T> Array
		=> (IList<T>?) ArrayField?.Value ?? Empty;

	private ArrayField<T>? ArrayField
	{
		get
		{
			var field = propertyBag.Get(propertyName);

			if (field is null)
				return null;

			if (field is not ArrayField<T> arrayField)
				throw new InvalidOperationException($"Property \"{propertyName}\" is not an Array<{nameof(T)}> field");

			return arrayField;
		}
	}

	public void ClearProperty()
		=> propertyBag.ClearProperty(propertyName);

	#region IList

	public T this[int index]
	{
		get => Array[index];
		set
		{
			EnsureFieldPresent();
			Array[index] = value;
		}
	}

	public int IndexOf(T item)
		=> Array.IndexOf(item);

	public void Insert(int index, T item)
	{
		EnsureFieldPresent();
		Array.Insert(index, item);
	}

	public void RemoveAt(int index)
		// Don't want the empty ReadOnlyCollection to throw if the field is absent
		=> ArrayField?.Value.RemoveAt(index);

	#endregion

	#region ICollection

	public int  Count      => Array.Count;
	public bool IsReadOnly => false;

	public void Add(T item)
	{
		EnsureFieldPresent();
		Array.Add(item);
	}

	public void Clear()
		// Don't want the empty ReadOnlyCollection to throw if the field is absent
		=> ArrayField?.Value.Clear();

	public bool Contains(T item)
		=> Array.Contains(item);

	public void CopyTo(T[] array, int index)
		=> Array.CopyTo(array, index);

	public bool Remove(T item)
		// Don't want the empty ReadOnlyCollection to throw if the field is absent
		=> ArrayField?.Value.Remove(item) ?? false;

	#endregion

	#region IEnumerable

	public IEnumerator<T> GetEnumerator()
		=> Array.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion

	private void EnsureFieldPresent()
	{
		if (ArrayField is null)
			propertyBag.SetValue(propertyName, new List<T>());
	}
}