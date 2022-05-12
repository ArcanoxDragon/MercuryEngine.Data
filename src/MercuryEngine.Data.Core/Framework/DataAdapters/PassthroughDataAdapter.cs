using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

/// <summary>
/// An <see cref="IDataAdapter{TStorage,TValue}"/> that passes through data of type <typeparamref name="T"/> without performing any conversion operations.
/// </summary>
public class PassthroughDataAdapter<T> : IDataAdapter<T, T>
where T : IBinaryDataType
{
	public static PassthroughDataAdapter<T> Instance { get; } = new();

	public T Get(T storage)
		// Return the source instance unchanged
		=> storage;

	public void Put(ref T storage, T value)
		// Copy value to "storage" reference
		=> storage = value;
}