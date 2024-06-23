namespace MercuryEngine.Data.Definitions.DreadTypes;

public class DreadDictionaryType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Dictionary;

	public string? KeyType   { get; set; }
	public string? ValueType { get; set; }
}