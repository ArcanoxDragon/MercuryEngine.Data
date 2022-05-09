namespace MercuryEngine.Data.Framework.DataAdapters;

/// <summary>
/// An object that manages the storage and retrieval of a certain type of value in a particular storage class.
/// </summary>
public interface IDataAdapter<TValue, in TStorage>
{
	/// <summary>
	/// Retrieves a value of type <typeparamref name="TValue"/> from the provided <paramref name="storage"/>.
	/// </summary>
	TValue Get(TStorage storage);

	/// <summary>
	/// Stores the provided <paramref name="value"/> in the provided <paramref name="storage"/>.
	/// </summary>
	void Put(TStorage storage, TValue value);
}