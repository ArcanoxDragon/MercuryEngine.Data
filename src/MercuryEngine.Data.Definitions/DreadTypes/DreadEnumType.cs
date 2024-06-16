namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadEnumType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Enum;

	public Dictionary<string, uint> Values { get; set; } = [];
}