using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadDictionaryTypeFactory : BaseDreadDataTypeFactory<DreadDictionaryType, DictionaryDataType<IBinaryDataType, IBinaryDataType>>
{
	public static DreadDictionaryTypeFactory Instance { get; } = new();

	protected override DictionaryDataType<IBinaryDataType, IBinaryDataType> CreateDataType(DreadDictionaryType dreadType)
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

		return new DictionaryDataType<IBinaryDataType, IBinaryDataType>(
			() => DreadTypeRegistry.CreateDataTypeFor(keyType),
			() => DreadTypeRegistry.CreateDataTypeFor(valueType)
		);
	}
}