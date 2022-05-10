using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Framework.DataAdapters;

public class BinaryDataTypeWithValueAdapter<TData, TValue> : IDataAdapter<TData, TValue>
where TData : IBinaryDataType<TValue>
where TValue : notnull
{
	public static BinaryDataTypeWithValueAdapter<TData, TValue> Instance { get; } = new();

	public TValue Get(TData storage)
		=> storage.Value;

	public void Put(TData storage, TValue value)
		=> storage.Value = value;
}