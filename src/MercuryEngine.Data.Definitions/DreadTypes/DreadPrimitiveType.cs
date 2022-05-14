namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadPrimitiveType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Primitive;

	public DreadPrimitiveKind PrimitiveKind { get; set; }
}