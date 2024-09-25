using System.Text.RegularExpressions;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators.Structs;

internal sealed record StructField(
	string FieldName,
	string FieldTypeName,
	string PropertyName,
	string PropertyType,
	string[] PropertyTypeGenericArgs,
	string ConfigureMethod,
	StructFieldKind StructFieldKind,
	IPropertySymbol? PreexistingProperty
) : IStructField
{
	#region Static

	private static readonly Regex GenericArgsRegex = new(@"^(\w+)<([\w\s,<>_]+)>$", RegexOptions.Compiled);

	#endregion

	public bool ShouldGenerate => PreexistingProperty is null;
	public bool HasSummary     => true;

	public string GenerateSummary()
		=> $"Field: {FieldName}&#10;Original type: {TypeNameUtility.XmlEscapeTypeName(FieldTypeName)}";

	public IEnumerable<string> GenerateProperty()
	{
		yield return $"public {PropertyType}? {PropertyName}";

		var allGenericArgs = string.Join(", ", PropertyTypeGenericArgs);

		switch (StructFieldKind)
		{
			case StructFieldKind.BasicValue:
				yield return "{";
				yield return $"\tget => RawFields.GetValue<{PropertyType}>(\"{FieldName}\");";
				yield return $"\tset => RawFields.SetOrClearValue(\"{FieldName}\", value);";
				yield return "}";
				break;
			case StructFieldKind.RawField:
				yield return "{";
				yield return $"\tget => RawFields.Get<{PropertyType}>(\"{FieldName}\");";
				yield return $"\tset => RawFields.SetOrClear(\"{FieldName}\", value);";
				yield return "}";
				break;
			case StructFieldKind.Array:
				yield return $"\t=> RawFields.Array<{allGenericArgs}>(\"{FieldName}\");";
				break;
			case StructFieldKind.Dictionary:
				yield return $"\t=> RawFields.Dictionary<{allGenericArgs}>(\"{FieldName}\");";
				break;
			default:
				yield return $"get => throw new NotSupportedException(\"Cannot access field \\\"{FieldName}\\\" due to a source generation error\")";
				break;
		}
	}

	public string GenerateDefine()
		=> StructFieldKind switch {
			StructFieldKind.BasicValue
				=> $"fields.{ConfigureMethod}(\"{FieldName}\");",

			StructFieldKind.RawField
				=> $"fields.{ConfigureMethod}(\"{FieldName}\", () => new {PropertyType}());",

			StructFieldKind.Array
				=> $"fields.{ConfigureMethod}(\"{FieldName}\", () => new {PropertyTypeGenericArgs[0]}());",

			StructFieldKind.Dictionary
				=> $"fields.{ConfigureMethod}(\"{FieldName}\", () => new {PropertyTypeGenericArgs[0]}(), () => new {PropertyTypeGenericArgs[1]}());",

			_ => $"throw new NotSupportedException(\"Cannot read or write field \\\"{FieldName}\\\".\");",
		};
}