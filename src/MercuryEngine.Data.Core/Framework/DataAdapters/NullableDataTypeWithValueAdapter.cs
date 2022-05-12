using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

public class NullableDataTypeWithValueAdapter<TData, TValue> : IDataAdapter<TData, TValue?>
where TData : IBinaryDataType<TValue>
where TValue : struct
{
	public static NullableDataTypeWithValueAdapter<TData, TValue> Instance { get; } = new();

	public TValue? Get(TData storage)
		=> storage.Value;

	public void Put(ref TData storage, TValue? value)
	{
		if (!value.HasValue)
			throw new ArgumentNullException(nameof(value));

		storage.Value = value.Value;
	}
}