using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadFlagsetTypeFactory : BaseDreadDataTypeFactory<DreadFlagsetType, IBinaryDataType>
{
	public static DreadFlagsetTypeFactory Instance { get; } = new();

	protected override IBinaryDataType CreateDataType(DreadFlagsetType dreadType)
	{
		var typeName = dreadType.TypeName;
		var enumTypeName = dreadType.Enum;

		if (enumTypeName is null)
			throw new InvalidOperationException($"Flagset type \"{typeName}\" is missing an enum name");

		if (!DreadTypeRegistry.TryFindType(enumTypeName, out var enumType))
			throw new InvalidOperationException($"Flagset type \"{typeName}\" has unknown enum type \"{enumTypeName}\"");

		return DreadTypeRegistry.CreateDataTypeFor(enumType);
	}
}