using JetBrains.Annotations;

namespace MercuryEngine.Data.SourceGenerators.Utility;

internal static class TypeNameUtility
{
	[ContractAnnotation("notnull => notnull")]
	public static string? SanitizeTypeName(string? typeName)
		=> typeName?.Replace("::", "__").Replace("<", "_").Replace(",", "_").Replace(">", "");

	public static string XmlEscapeTypeName(string typeName)
		=> typeName.Replace("<", "&lt;").Replace(">", "&gt;");
}