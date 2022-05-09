using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadConcreteType<T> : BaseDreadType
where T : IBinaryDataType, new()
{
	public DreadConcreteType(string typeName)
	{
		TypeName = typeName;
	}

	public override DreadTypeKind Kind => DreadTypeKind.Concrete;

	public override IBinaryDataType CreateDataType()
		=> new T();
}