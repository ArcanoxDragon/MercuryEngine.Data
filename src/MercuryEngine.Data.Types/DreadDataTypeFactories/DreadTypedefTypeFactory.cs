using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadTypedefTypeFactory : BaseDreadDataTypeFactory<DreadTypedefType, IBinaryDataType>
{
	public static DreadTypedefTypeFactory Instance { get; } = new();

	protected override IBinaryDataType CreateDataType(DreadTypedefType dreadType)
	{
		var typeName = dreadType.TypeName;
		var aliasTypeName = dreadType.Alias;

		if (aliasTypeName is null)
			throw new InvalidOperationException($"Typedef type \"{typeName}\" is missing an alias");

		if (!DreadTypeRegistry.TryFindType(aliasTypeName, out var aliasedType))
			throw new InvalidOperationException($"Typedef type \"{typeName}\" refers to unknown type \"{aliasTypeName}\"");

		return DreadTypeRegistry.CreateDataTypeFor(aliasedType);
	}
}