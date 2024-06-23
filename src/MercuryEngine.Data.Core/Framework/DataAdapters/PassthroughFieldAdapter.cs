using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

/// <summary>
/// An <see cref="IFieldAdapter{TStorage,TValue}"/> that passes through data of type <typeparamref name="T"/> without performing any conversion operations.
/// </summary>
public class PassthroughFieldAdapter<T> : IFieldAdapter<T, T>
where T : IBinaryField
{
	public static PassthroughFieldAdapter<T> Instance { get; } = new();

	public T Get(T storage)
		// Return the source instance unchanged
		=> storage;

	public void Put(ref T storage, T value)
		// Copy value to "storage" reference
		=> storage = value;
}