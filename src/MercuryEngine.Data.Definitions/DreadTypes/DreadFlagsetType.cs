using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadFlagsetType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Flagset;

	public string? Enum { get; set; }

	public override IBinaryDataType CreateDataType()
	{
		if (Enum is null)
			throw new InvalidOperationException($"Typedef type \"{TypeName}\" is missing an enum name");

		if (!DreadTypeRegistry.TryFindType(Enum, out var enumType))
			throw new InvalidOperationException($"Typedef type \"{TypeName}\" has unknown enum type \"{Enum}\"");

		return enumType.CreateDataType();
	}
}