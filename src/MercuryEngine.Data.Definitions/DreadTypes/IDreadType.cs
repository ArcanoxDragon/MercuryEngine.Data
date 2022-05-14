namespace MercuryEngine.Data.Definitions.DreadTypes;

public interface IDreadType
{
	string        TypeName { get; }
	DreadTypeKind Kind     { get; }
}