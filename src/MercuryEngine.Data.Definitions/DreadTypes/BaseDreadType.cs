namespace MercuryEngine.Data.Definitions.DreadTypes;

public abstract class BaseDreadType : IDreadType
{
	public string TypeName { get; set; } = string.Empty;

	public abstract DreadTypeKind Kind { get; }
}