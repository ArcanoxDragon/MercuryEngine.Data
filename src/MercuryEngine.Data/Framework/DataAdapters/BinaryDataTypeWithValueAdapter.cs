using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Framework.DataAdapters;

public class BinaryDataTypeWithValueAdapter<TValue, TData> : IDataAdapter<TValue, TData>
where TValue : notnull
where TData : IBinaryDataType<TValue>
{
	public TValue Get(TData storage)
		=> storage.Value;

	public void Put(TData storage, TValue value)
		=> storage.Value = value;
}