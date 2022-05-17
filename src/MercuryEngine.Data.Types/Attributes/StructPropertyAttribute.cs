namespace MercuryEngine.Data.Types.Attributes;

/// <summary>
/// Used to indicate that a property defined manually in a partial type class is intended to act as the backing property for a particular struct field.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StructPropertyAttribute : Attribute
{
	/// <summary>
	/// Constructs a new instance of <see cref="StructPropertyAttribute"/>.
	/// </summary>
	/// <param name="fieldName">The name of the struct field that the property on which this attribute is placed represents.</param>
	public StructPropertyAttribute(string fieldName)
	{
		FieldName = fieldName;
	}

	/// <summary>
	/// Gets the name of the struct field that the property on which this attribute is placed represents.
	/// </summary>
	public string FieldName { get; }
}