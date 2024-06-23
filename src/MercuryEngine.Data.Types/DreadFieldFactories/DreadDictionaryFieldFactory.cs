using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadDictionaryFieldFactory : BaseDreadFieldFactory<DreadDictionaryType, DictionaryField<IBinaryField, IBinaryField>>
{
	public static DreadDictionaryFieldFactory Instance { get; } = new();

	protected override DictionaryField<IBinaryField, IBinaryField> CreateField(DreadDictionaryType dreadType)
	{
		var typeName = dreadType.TypeName;
		var keyTypeName = dreadType.KeyType;
		var valueTypeName = dreadType.ValueType;

		if (keyTypeName is null)
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" is missing a value type");
		if (valueTypeName is null)
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" is missing a value type");

		if (!DreadTypeRegistry.TryFindType(keyTypeName, out var keyType))
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" has unknown key type \"{keyTypeName}\"");
		if (!DreadTypeRegistry.TryFindType(valueTypeName, out var valueType))
			throw new InvalidOperationException($"Dictionary type \"{typeName}\" has unknown value type \"{valueTypeName}\"");

		return new DictionaryField<IBinaryField, IBinaryField>(
			() => DreadTypeRegistry.GetFieldForType(keyType),
			() => DreadTypeRegistry.GetFieldForType(valueType)
		);
	}
}