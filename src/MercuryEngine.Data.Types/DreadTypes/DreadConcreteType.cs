using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadTypes;

public class DreadConcreteType : BaseDreadType
{
	private readonly Func<IBinaryField> dataTypeFactory;

	public DreadConcreteType(string typeName, Func<IBinaryField> dataTypeFactory)
	{
		TypeName = typeName;

		this.dataTypeFactory = dataTypeFactory;
	}

	public override DreadTypeKind Kind => DreadTypeKind.Concrete;

	internal IBinaryField CreateDataType() => this.dataTypeFactory();
}