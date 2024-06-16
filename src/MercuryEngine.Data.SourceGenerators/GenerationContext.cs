using MercuryEngine.Data.Definitions.DreadTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MercuryEngine.Data.SourceGenerators;

public record GenerationContext(IReadOnlyDictionary<string, BaseDreadType> KnownTypes) : ISyntaxContextReceiver
{
	public List<PreexistingType> PreexistingTypes { get; } = [];
	public List<GeneratedType>   GeneratedTypes   { get; } = [];

	public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
	{
		switch (context.Node)
		{
			case ClassDeclarationSyntax classDeclaration:
				VisitClassDeclaration(context, classDeclaration);
				break;
		}
	}

	private void VisitClassDeclaration(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclaration)
	{
		if (classDeclaration is not { Parent: BaseNamespaceDeclarationSyntax namespaceDeclaration })
			return;

		var namespaceName = namespaceDeclaration.Name.GetText().ToString();
		var className = classDeclaration.Identifier.ValueText;

		if (namespaceName is not Constants.DreadTypesNamespace)
			return;

		var declaredProperties = classDeclaration.Members.OfType<BasePropertyDeclarationSyntax>();
		var typeProperties = declaredProperties.Select(p => context.SemanticModel.GetDeclaredSymbol(p)).Cast<IPropertySymbol>();

		PreexistingTypes.Add(new PreexistingType(classDeclaration, namespaceName, className, typeProperties.ToList()));
	}
}