using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadVectorType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Vector;

	public string? ValueType { get; set; }

	public override IBinaryDataType CreateDataType()
	{
		if (ValueType is null)
			throw new InvalidOperationException($"Vector type \"{TypeName}\" is missing a value type");

		if (!DreadTypeRegistry.TryFindType(ValueType, out var valueType))
			throw new InvalidOperationException($"Vector type \"{TypeName}\" has unknown value type \"{ValueType}\"");

		return new ArrayDataType<IBinaryDataType>(() => valueType.CreateDataType());
	}
}