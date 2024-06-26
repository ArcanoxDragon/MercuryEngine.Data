using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadTypedefFieldFactory : BaseDreadFieldFactory<DreadTypedefType, IBinaryField>
{
	public static DreadTypedefFieldFactory Instance { get; } = new();

	protected override IBinaryField CreateField(DreadTypedefType dreadType)
	{
		var typeName = dreadType.TypeName;
		var aliasTypeName = dreadType.Alias;

		if (aliasTypeName is null)
			throw new InvalidOperationException($"Typedef type \"{typeName}\" is missing an alias");

		if (!DreadTypeRegistry.TryFindType(aliasTypeName, out var aliasedType))
			throw new InvalidOperationException($"Typedef type \"{typeName}\" refers to unknown type \"{aliasTypeName}\"");

		return DreadTypeRegistry.CreateFieldForType(aliasedType);
	}
}