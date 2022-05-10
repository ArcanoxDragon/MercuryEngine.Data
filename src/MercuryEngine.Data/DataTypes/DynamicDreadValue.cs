using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Utility.DreadTypeHelpers;

namespace MercuryEngine.Data.DataTypes;

public class DynamicDreadValue
{
	public DynamicDreadValue(BaseDreadType dreadType)
		: this(dreadType.TypeName, dreadType.CreateDataType()) { }

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