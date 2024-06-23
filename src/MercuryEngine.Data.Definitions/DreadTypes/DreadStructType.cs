namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadStructType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Struct;

	public string?                    Parent { get; set; }
	public Dictionary<string, string> Fields { get; set; } = [];
}