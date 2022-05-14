namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadFlagsetType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Flagset;

	public string? Enum { get; set; }
}