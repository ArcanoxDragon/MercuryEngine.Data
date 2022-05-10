using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadDictionaryType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Dictionary;

	public string? KeyType   { get; set; }
	public string? ValueType { get; set; }

	public override IBinaryDataType CreateDataType()
	{
		if (KeyType is null)
			throw new InvalidOperationException($"Dictionary type \"{TypeName}\" is missing a value type");
		if (ValueType is null)
			throw new InvalidOperationException($"Dictionary type \"{TypeName}\" is missing a value type");

		if (!DreadTypes.TryFindType(KeyType, out var keyType))
			throw new InvalidOperationException($"Dictionary type \"{TypeName}\" has unknown key type \"{KeyType}\"");
		if (!DreadTypes.TryFindType(ValueType, out var valueType))
			throw new InvalidOperationException($"Dictionary type \"{TypeName}\" has unknown value type \"{ValueType}\"");

		return new ArrayDataType<KeyValuePairDataType<IBinaryDataType, IBinaryDataType>>(() => {
			var keyData = keyType.CreateDataType();
			var valueData = valueType.CreateDataType();

			return new KeyValuePairDataType<IBinaryDataType, IBinaryDataType>(keyData, valueData);
		});
	}
}