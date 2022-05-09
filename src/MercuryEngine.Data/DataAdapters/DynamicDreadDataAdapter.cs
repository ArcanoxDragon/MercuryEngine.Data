using MercuryEngine.Data.DataTypes;
using MercuryEngine.Data.Framework.DataAdapters;
using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.DataAdapters;

public class DynamicDreadDataAdapter : IDataAdapter<IBinaryDataType, DynamicDreadDataType>
{
	public IBinaryDataType Get(DynamicDreadDataType storage)
		=> storage.RawData ?? throw new InvalidOperationException($"Cannot retrieve values from an uninitialized {nameof(DynamicDreadDataType)}");

	public void Put(DynamicDreadDataType storage, IBinaryDataType value)
		=> throw new NotSupportedException($"Cannot set value on a {nameof(DynamicDreadDataType)} field");
}