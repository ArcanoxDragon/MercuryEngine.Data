using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MercuryEngine.Data.SourceGenerators;

internal record PreexistingType(BaseTypeDeclarationSyntax Declaration, string Namespace, string Name, IReadOnlyCollection<IPropertySymbol> Properties);