using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadEnumFieldFactory : BaseDreadFieldFactory<DreadEnumType, IBinaryField>
{
	public static DreadEnumFieldFactory Instance { get; } = new();

	private static readonly Dictionary<string, EnumTypeFactory> ConcreteEnumTypes = [];

	public static void RegisterConcreteEnumType<T>(string name)
	where T : struct, Enum
		=> ConcreteEnumTypes.Add(name, () => new DreadEnum<T>());

	protected override IBinaryField CreateField(DreadEnumType dreadType)
	{
		if (ConcreteEnumTypes.TryGetValue(dreadType.TypeName, out var concreteEnumTypeFactory))
			return concreteEnumTypeFactory();

		return new Int32Field();
	}

	private delegate IBinaryField EnumTypeFactory();
}