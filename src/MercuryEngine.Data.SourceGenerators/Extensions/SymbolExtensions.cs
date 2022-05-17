using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Extensions;

internal static class SymbolExtensions
{
	public static string GetFullyQualifiedName(this ISymbol symbol)
	{
		if (symbol.ContainingNamespace is null or { IsGlobalNamespace: true })
			return symbol.Name;

		return $"{symbol.ContainingNamespace.GetFullyQualifiedName()}.{symbol.Name}";
	}

	public static string GetSimpleGenericName(this ITypeSymbol symbol)
	{
		if (symbol is not INamedTypeSymbol { TypeArguments.Length: > 0 } namedSymbol)
			return symbol.Name;

		var simpleTypeArgumentNames = namedSymbol.TypeArguments.Select(t => t.GetSimpleGenericName());

		return $"{symbol.Name}<{string.Join(", ", simpleTypeArgumentNames)}>";
	}
}