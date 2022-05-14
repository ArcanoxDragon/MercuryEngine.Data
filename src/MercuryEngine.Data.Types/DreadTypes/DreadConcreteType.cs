using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadTypes;

public class DreadConcreteType : BaseDreadType
{
	private readonly Func<IBinaryDataType> dataTypeFactory;

	public DreadConcreteType(string typeName, Func<IBinaryDataType> dataTypeFactory)
	{
		TypeName = typeName;

		this.dataTypeFactory = dataTypeFactory;
	}

	public override DreadTypeKind Kind => DreadTypeKind.Concrete;

	internal IBinaryDataType CreateDataType() => this.dataTypeFactory();
}