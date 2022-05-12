using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

public class DataTypeWithValueAdapter<TData, TValue> : IDataAdapter<TData, TValue>
where TData : IBinaryDataType<TValue>
where TValue : notnull
{
	public static DataTypeWithValueAdapter<TData, TValue> Instance { get; } = new();

	public TValue Get(TData storage)
		=> storage.Value;

	public void Put(ref TData storage, TValue value)
		=> storage.Value = value;
}