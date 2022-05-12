using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadTypedefType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Typedef;

	public string? Alias { get; set; }

	public override IBinaryDataType CreateDataType()
	{
		if (Alias is null)
			throw new InvalidOperationException($"Typedef type \"{TypeName}\" is missing an alias");

		if (!DreadTypeRegistry.TryFindType(Alias, out var aliasedType))
			throw new InvalidOperationException($"Typedef type \"{TypeName}\" refers to unknown type \"{Alias}\"");

		return aliasedType.CreateDataType();
	}
}