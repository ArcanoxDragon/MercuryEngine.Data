using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadVectorFieldFactory : BaseDreadFieldFactory<DreadVectorType, DreadArrayField<IBinaryField>>
{
	public static DreadVectorFieldFactory Instance { get; } = new();

	protected override DreadArrayField<IBinaryField> CreateField(DreadVectorType dreadType)
	{
		var typeName = dreadType.TypeName;
		var valueTypeName = dreadType.ValueType;

		if (valueTypeName is null)
			throw new InvalidOperationException($"Vector type \"{typeName}\" is missing a value type");

		if (!DreadTypeLibrary.TryFindType(valueTypeName, out var valueType))
			throw new InvalidOperationException($"Vector type \"{typeName}\" has unknown value type \"{valueTypeName}\"");

		return new DreadArrayField<IBinaryField>(typeName, () => DreadTypeLibrary.CreateFieldForType(valueType));
	}
}