using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadTypes;

public class DreadConcreteType : BaseDreadType
{
	private readonly Func<IBinaryField> dataTypeFactory;

	public DreadConcreteType(string typeName, string? parentTypeName, Func<IBinaryField> dataTypeFactory)
	{
		TypeName = typeName;
		Parent = parentTypeName;

		this.dataTypeFactory = dataTypeFactory;
	}

	public override DreadTypeKind Kind => DreadTypeKind.Concrete;

	public string? Parent { get; }

	internal IBinaryField CreateDataType() => this.dataTypeFactory();
}