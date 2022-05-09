using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadFlagsetType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Flagset;

	public string? Enum { get; set; }

	public override IBinaryDataType CreateDataType()
	{
		if (Enum is null)
			throw new InvalidOperationException($"Typedef type \"{TypeName}\" is missing an enum name");

		if (DreadTypes.FindType(Enum) is not { } enumType)
			throw new InvalidOperationException($"Typedef type \"{TypeName}\" has unknown enum type \"{Enum}\"");

		return enumType.CreateDataType();
	}
}