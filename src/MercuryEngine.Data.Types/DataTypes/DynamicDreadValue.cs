using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DataTypes;

public class DynamicDreadValue
{
	public DynamicDreadValue(BaseDreadType dreadType)
		: this(dreadType.TypeName, DreadTypeRegistry.CreateDataTypeFor(dreadType)) { }

	public DynamicDreadValue(string typeName, IBinaryDataType data)
	{
		TypeName = typeName;
		TypeId = TypeName.GetCrc64();
		Data = data;
	}

	public string          TypeName { get; }
	public ulong           TypeId   { get; }
	public IBinaryDataType Data     { get; }
}