using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators;

internal static class Constants
{
	public const string AttributesNamespace              = "MercuryEngine.Data.Types.Attributes";
	public const string StructPropertyAttributeClassName = $"{AttributesNamespace}.StructPropertyAttribute";
	public const string DreadTypesNamespace              = "MercuryEngine.Data.Types.DreadTypes";
	public const string DreadTypeLibraryNamespace        = "MercuryEngine.Data.Types";
	public const string DreadTypeLibraryClassName        = "DreadTypeLibrary";

	public static class Diagnostics
	{
		public static readonly DiagnosticDescriptor PropertyTypeMismatchDescriptor = new(
			"MD1001",
			"Property type does not match expected type",
			"Property \"{0}\" on type \"{1}\" must have a type of \"{2}\" in order to be used for struct field \"{3}\"",
			"MercuryEngine.Data.DreadTypes",
			DiagnosticSeverity.Error,
			true);

		public static readonly DiagnosticDescriptor PropertyMissingGetterOrSetterDescriptor = new(
			"MD1002",
			"Property is missing a getter or a setter",
			"Property \"{0}\" on type \"{1}\" must have both a getter and a setter in order to be used for struct field \"{2}\"",
			"MercuryEngine.Data.DreadTypes",
			DiagnosticSeverity.Error,
			true);
	}
}