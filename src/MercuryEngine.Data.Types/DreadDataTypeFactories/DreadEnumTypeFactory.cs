using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.DataTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadEnumTypeFactory : BaseDreadDataTypeFactory<DreadEnumType, IBinaryDataType>
{
	public static DreadEnumTypeFactory Instance { get; } = new();

	private static readonly Dictionary<string, EnumTypeFactory> ConcreteEnumTypes = [];

	public static void RegisterConcreteEnumType<T>(string name)
	where T : struct, Enum
		=> ConcreteEnumTypes.Add(name, () => new DreadEnum<T>());

	protected override IBinaryDataType CreateDataType(DreadEnumType dreadType)
	{
		if (ConcreteEnumTypes.TryGetValue(dreadType.TypeName, out var concreteEnumTypeFactory))
			return concreteEnumTypeFactory();

		return new Int32DataType();
	}

	private delegate IBinaryDataType EnumTypeFactory();
}