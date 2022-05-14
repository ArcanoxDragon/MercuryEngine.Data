namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadPointerType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Pointer;

	public string? Target { get; set; }
}