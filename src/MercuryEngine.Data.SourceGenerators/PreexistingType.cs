using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MercuryEngine.Data.SourceGenerators;

public record PreexistingType(BaseTypeDeclarationSyntax Declaration, string Namespace, string Name, IReadOnlyCollection<IPropertySymbol> Properties);