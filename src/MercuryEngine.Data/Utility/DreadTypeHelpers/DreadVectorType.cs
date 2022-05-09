using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadVectorType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Vector;

	public string? ValueType { get; set; }

	public override IBinaryDataType CreateDataType()
	{
		if (ValueType is null)
			throw new InvalidOperationException($"Vector type \"{TypeName}\" is missing a value type");

		if (DreadTypes.FindType(ValueType) is not { } valueType)
			throw new InvalidOperationException($"Vector type \"{TypeName}\" has unknown value type \"{ValueType}\"");

		return new ArrayDataType<IBinaryDataType>(() => valueType.CreateDataType());
	}
}