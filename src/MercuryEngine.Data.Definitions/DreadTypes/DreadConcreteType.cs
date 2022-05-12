using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadConcreteType<T> : BaseDreadType
where T : IBinaryDataType
{
	private readonly Func<T> dataTypeFactory;

	public DreadConcreteType(Func<T> dataTypeFactory, string typeName)
	{
		this.dataTypeFactory = dataTypeFactory;
		TypeName = typeName;
	}

	public override DreadTypeKind Kind => DreadTypeKind.Concrete;

	public override IBinaryDataType CreateDataType()
		=> this.dataTypeFactory();
}