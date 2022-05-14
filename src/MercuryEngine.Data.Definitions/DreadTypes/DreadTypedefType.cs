namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadTypedefType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Typedef;

	public string? Alias { get; set; }
}