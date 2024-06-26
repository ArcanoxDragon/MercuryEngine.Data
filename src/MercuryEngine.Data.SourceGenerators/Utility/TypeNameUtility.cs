using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Utility;

internal static class TypeNameUtility
{
	[return: NotNullIfNotNull(nameof(typeName))]
	public static string? SanitizeTypeName(string? typeName)
		=> typeName?.Replace("::", "__").Replace("<", "_").Replace(",", "_").Replace(">", "");

	public static string XmlEscapeTypeName(string typeName)
		=> typeName.Replace("<", "&lt;").Replace(">", "&gt;");
}