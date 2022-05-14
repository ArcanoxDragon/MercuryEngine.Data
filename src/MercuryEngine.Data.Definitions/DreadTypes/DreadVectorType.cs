namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadVectorType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Vector;

	public string? ValueType { get; set; }
}