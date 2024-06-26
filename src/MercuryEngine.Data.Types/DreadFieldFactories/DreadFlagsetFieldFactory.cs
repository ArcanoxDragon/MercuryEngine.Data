using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadFlagsetFieldFactory : BaseDreadFieldFactory<DreadFlagsetType, IBinaryField>
{
	public static DreadFlagsetFieldFactory Instance { get; } = new();

	protected override IBinaryField CreateField(DreadFlagsetType dreadType)
	{
		var typeName = dreadType.TypeName;
		var enumTypeName = dreadType.Enum;

		if (enumTypeName is null)
			throw new InvalidOperationException($"Flagset type \"{typeName}\" is missing an enum name");

		if (!DreadTypeRegistry.TryFindType(enumTypeName, out var enumType))
			throw new InvalidOperationException($"Flagset type \"{typeName}\" has unknown enum type \"{enumTypeName}\"");

		return DreadTypeRegistry.CreateFieldForType(enumType);
	}
}