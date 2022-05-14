using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadVectorTypeFactory : BaseDreadDataTypeFactory<DreadVectorType, ArrayDataType<IBinaryDataType>>
{
	public static DreadVectorTypeFactory Instance { get; } = new();

	protected override ArrayDataType<IBinaryDataType> CreateDataType(DreadVectorType dreadType)
	{
		var typeName = dreadType.TypeName;
		var valueTypeName = dreadType.ValueType;

		if (valueTypeName is null)
			throw new InvalidOperationException($"Vector type \"{typeName}\" is missing a value type");

		if (!DreadTypeRegistry.TryFindType(valueTypeName, out var valueType))
			throw new InvalidOperationException($"Vector type \"{typeName}\" has unknown value type \"{valueTypeName}\"");

		return new ArrayDataType<IBinaryDataType>(() => DreadTypeRegistry.CreateDataTypeFor(valueType));
	}
}