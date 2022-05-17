namespace MercuryEngine.Data.Types.Attributes;

[AttributeUsage(AttributeTargets.Enum)]
public class DreadEnumAttribute : Attribute
{
	public DreadEnumAttribute(string typeName)
	{
		TypeName = typeName;
	}

	public string TypeName { get; }
}