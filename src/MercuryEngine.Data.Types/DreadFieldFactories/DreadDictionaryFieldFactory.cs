using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadDictionaryFieldFactory : BaseDreadFieldFactory<DreadDictionaryType, DreadDictionaryField<IBinaryField, IBinaryField>>
{
	public static DreadDictionaryFieldFactory Instance { get; } = new();

	protected override DreadDictionaryField<IBinaryField, IBinaryField> CreateField(DreadDictionaryType dreadType)
	{
		var typeName = dreadType.TypeName;
		var keyTypeName = dreadType.KeyType;
		var valueTypeName = dreadType.ValueType;

		if (keyTypeName is null)
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" is missing a key type");
		if (valueTypeName is null)
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" is missing a value type");

		if (!DreadTypeLibrary.TryFindType(keyTypeName, out var keyType))
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" has unknown key type \"{keyTypeName}\"");
		if (!DreadTypeLibrary.TryFindType(valueTypeName, out var valueType))
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" has unknown value type \"{valueTypeName}\"");

		return new DreadDictionaryField<IBinaryField, IBinaryField>(
			typeName,
			() => DreadTypeLibrary.CreateFieldForType(keyType),
			() => DreadTypeLibrary.CreateFieldForType(valueType)
		);
	}
}