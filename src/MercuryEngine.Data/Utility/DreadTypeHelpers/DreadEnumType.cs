using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadEnumType : BaseDreadType
{
	private delegate IBinaryDataType EnumTypeFactory();

	private static readonly Dictionary<string, EnumTypeFactory> ConcreteEnumTypes = new();

	public static void RegisterConcreteEnumType<T>(string name)
	where T : struct, Enum
		=> ConcreteEnumTypes.Add(name, () => new EnumDataType<T>());

	public override DreadTypeKind Kind => DreadTypeKind.Enum;

	public Dictionary<string, uint> Values { get; set; } = new();

	public override IBinaryDataType CreateDataType()
	{
		if (ConcreteEnumTypes.TryGetValue(TypeName, out var concreteEnumTypeFactory))
			return concreteEnumTypeFactory();

		return new Int32DataType();
	}
}