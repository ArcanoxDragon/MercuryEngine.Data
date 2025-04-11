using System.Collections.Immutable;
using System.Text;
using MercuryEngine.Data.Definitions.Utility;
using MercuryEngine.Data.SourceGenerators.Generators;
using MercuryEngine.Data.SourceGenerators.Generators.Structs;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if DEBUG
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Definitions.Extensions;
#endif

namespace MercuryEngine.Data.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public class DreadTypesSourceGenerator : IIncrementalGenerator
{
	private readonly DreadEnumGenerator    enumGenerator    = new();
	private readonly DreadFlagsetGenerator flagsetGenerator = new();
	private readonly DreadStructGenerator  structGenerator  = new();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var dreadTypesProvider = context.AdditionalTextsProvider
			.Where(static file => file.Path.EndsWith("dread_types.json"))
			.Collect()
			.Select((files, cancellationToken) => {
				try
				{
					var sourceText = files.Length == 0 ? null : files[0].GetText(cancellationToken);

					if (sourceText is null)
						throw new IOException("Failed to obtain SourceText for dread_types.json");

					return new DreadTypesParseResult(DreadTypeParser.ParseDreadTypes(sourceText.ToString()), sourceText);
				}
				catch (Exception ex)
				{
					return new DreadTypesParseResult([], null, ex);
				}
			})
			.WithComparer(DreadTypesParseResultEqualityComparer.Instance);

		var typeNamesMappingInputsProvider = context.AnalyzerConfigOptionsProvider
			.Select(static (configOptions, _) => {
				if (configOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir))
					return projectDir;

				return null;
			})
			.Combine(dreadTypesProvider);

		context.RegisterSourceOutput(typeNamesMappingInputsProvider, GenerateTypeNamesMappingSourceLines);

		var preexistingTypesProvider = context.SyntaxProvider
			.CreateSyntaxProvider(IsPreexistingDreadType, GetPreexistingType)
			.Collect();

		var generationContextProvider = dreadTypesProvider
			.Combine(preexistingTypesProvider)
			.Select(static (values, _) => {
				var (parseResult, preexistingTypes) = values;

				return new GenerationContext(parseResult.DreadTypes, preexistingTypes);
			});

		var enumGeneratedTypesProvider = dreadTypesProvider.SelectMany((parseResult, _) => this.enumGenerator.GetTypesToGenerate(parseResult.DreadTypes));
		var enumGeneratorInputsProvider = enumGeneratedTypesProvider.Combine(generationContextProvider);

		context.RegisterSourceOutput(enumGeneratorInputsProvider, (productionContext, inputs) => {
			GenerateTypeFileWith(this.enumGenerator, productionContext, inputs.Left, inputs.Right);
		});

		var flagsetGeneratedTypesProvider = dreadTypesProvider.SelectMany((parseResult, _) => this.flagsetGenerator.GetTypesToGenerate(parseResult.DreadTypes));
		var flagsetGeneratorInputsProvider = flagsetGeneratedTypesProvider.Combine(generationContextProvider);

		context.RegisterSourceOutput(flagsetGeneratorInputsProvider, (productionContext, inputs) => {
			GenerateTypeFileWith(this.flagsetGenerator, productionContext, inputs.Left, inputs.Right);
		});

		var structGeneratedTypesProvider = dreadTypesProvider.SelectMany((parseResult, _) => this.structGenerator.GetTypesToGenerate(parseResult.DreadTypes));
		var structGeneratorInputsProvider = structGeneratedTypesProvider.Combine(generationContextProvider);

		context.RegisterSourceOutput(structGeneratorInputsProvider, (productionContext, inputs) => {
			GenerateTypeFileWith(this.structGenerator, productionContext, inputs.Left, inputs.Right);
		});

		var allGeneratedTypesProvider = enumGeneratedTypesProvider.Collect()
			.Combine(flagsetGeneratedTypesProvider.Collect())
			.Select((tuple, _) => tuple.Left.AddRange(tuple.Right))
			.Combine(structGeneratedTypesProvider.Collect())
			.Select((tuple, _) => tuple.Left.AddRange(tuple.Right));

		context.RegisterImplementationSourceOutput(allGeneratedTypesProvider, GenerateRegistryPartialFile);
	}

	private static bool IsPreexistingDreadType(SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		if (syntaxNode is not ClassDeclarationSyntax { Parent: BaseNamespaceDeclarationSyntax namespaceDeclaration })
			return false;

		var namespaceName = namespaceDeclaration.Name.GetText().ToString();

		cancellationToken.ThrowIfCancellationRequested();

		return namespaceName is Constants.DreadTypesNamespace;
	}

	private static PreexistingType GetPreexistingType(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		if (context.Node is not ClassDeclarationSyntax { Parent: BaseNamespaceDeclarationSyntax namespaceDeclaration } classDeclaration)
			throw new InvalidOperationException($"Source generator received a syntax node ({context.Node}) that it did not subscribe to!");

		var namespaceName = namespaceDeclaration.Name.GetText().ToString();
		var className = classDeclaration.Identifier.ValueText;
		var declaredProperties = classDeclaration.Members.OfType<BasePropertyDeclarationSyntax>();
		var typeProperties = declaredProperties.Select(p => context.SemanticModel.GetDeclaredSymbol(p, cancellationToken)).Cast<IPropertySymbol>();

		return new PreexistingType(classDeclaration, namespaceName, className, [..typeProperties]);
	}

	private static void GenerateTypeFileWith(IDreadGenerator generator, SourceProductionContext productionContext, GeneratedType generatedType, GenerationContext generationContext)
	{
		string typeSourceText;
		bool isError = false;

		try
		{
			typeSourceText = generator.GenerateSource(generatedType, productionContext, generationContext);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			var stackTraceLines = ex.ToString().Split('\n').Select(line => $"// {line}");

			typeSourceText = $"// ERROR: Exception while generating type \"{generatedType.DreadTypeName}\": {string.Join("\n", stackTraceLines)}";
			isError = true;
		}

		var sourceTextBuilder = new StringBuilder();

		sourceTextBuilder.AppendLine("#nullable enable");

		foreach (var line in GetStandardNamespaceImports())
			sourceTextBuilder.AppendLine(line);

		sourceTextBuilder.AppendLine($"namespace {Constants.DreadTypesNamespace};");

		if (!isError)
		{
			sourceTextBuilder.AppendLine("/// <summary>");
			sourceTextBuilder.AppendLine($"/// Original type name: {TypeNameUtility.XmlEscapeTypeName(generatedType.DreadTypeName)}&#10;");
			sourceTextBuilder.AppendLine($"/// Kind: {generatedType.DreadType.Kind}");
			sourceTextBuilder.AppendLine("/// </summary>");
		}

		sourceTextBuilder.AppendLine(typeSourceText);

		productionContext.CancellationToken.ThrowIfCancellationRequested();

		var fileName = $"{TypeNameUtility.SanitizeTypeName(generatedType.DreadTypeName)}.g.cs";

		productionContext.AddSource(fileName, sourceTextBuilder.ToString());
	}

	private static void GenerateRegistryPartialFile(SourceProductionContext productionContext, ImmutableArray<GeneratedType> generatedTypes)
	{
		var sourceTextBuilder = new StringBuilder();

		sourceTextBuilder.AppendLine("#nullable enable");

		foreach (var line in GetStandardNamespaceImports())
			sourceTextBuilder.AppendLine(line);

		sourceTextBuilder.AppendLine($"using {Constants.DreadTypesNamespace};");

		sourceTextBuilder.AppendLine($"namespace {Constants.DreadTypeLibraryNamespace};");

		sourceTextBuilder.AppendLine($"public partial class {Constants.DreadTypeLibraryClassName}");
		sourceTextBuilder.AppendLine("{");

		sourceTextBuilder.AppendLine("\tstatic partial void RegisterGeneratedTypes()");
		sourceTextBuilder.AppendLine("\t{");

		foreach (var (_, csharpTypeName, dreadTypeName, parentTypeName) in generatedTypes)
		{
			if (parentTypeName != null)
				sourceTextBuilder.AppendLine($"\t\tDreadTypeLibrary.RegisterConcreteType<{csharpTypeName}>(\"{dreadTypeName}\", \"{parentTypeName}\");");
			else
				sourceTextBuilder.AppendLine($"\t\tDreadTypeLibrary.RegisterConcreteType<{csharpTypeName}>(\"{dreadTypeName}\");");
		}

		sourceTextBuilder.AppendLine("\t}");

		sourceTextBuilder.AppendLine("}");

		productionContext.AddSource("DreadGeneratedTypeLibrary.g.cs", sourceTextBuilder.ToString());
	}

	private static void GenerateTypeNamesMappingSourceLines(SourceProductionContext productionContext, (string? Left, DreadTypesParseResult Right) valueTuple)
	{
		// ReSharper disable once UnusedVariable
		var (projectDir, parseResult) = valueTuple;

		if (parseResult.Exception is { } exception)
		{
			productionContext.ReportDiagnostic(Diagnostic.Create(Constants.Diagnostics.CouldNotReadDreadTypesDiagnostic, null, exception));
			// ReSharper disable once RedundantJumpStatement
			return;
		}

#if DEBUG
#pragma warning disable RS1035 // We need direct file I/O if we want to write non-C# output files
		if (string.IsNullOrEmpty(projectDir) || !Directory.Exists(projectDir))
			return;

		var sourceTextBuilder = new StringBuilder();

		sourceTextBuilder.AppendLine("{");

		foreach (var (typeName, _) in parseResult.DreadTypes)
		{
			sourceTextBuilder.AppendLine($"\"{typeName}\": {{");

			var crc64 = typeName.GetCrc64();
			var hex = crc64.ToHexString();

			sourceTextBuilder.AppendLine($"\t\"crc_ulong\": {crc64},");
			sourceTextBuilder.AppendLine($"\t\"crc_hex\": \"{hex}\"");

			sourceTextBuilder.AppendLine("},");
		}

		sourceTextBuilder.AppendLine("}");

		// Need to write to the file directly, since the source generator API can only produce C# outputs
		File.WriteAllText(Path.Combine(projectDir, "dread_type_names.g.json"), sourceTextBuilder.ToString());
#pragma warning restore RS1035
#endif
	}

	private static IEnumerable<string> GetStandardNamespaceImports()
	{
		yield return "using System;";
		yield return "using MercuryEngine.Data.Core.Extensions;";
		yield return "using MercuryEngine.Data.Core.Framework;";
		yield return "using MercuryEngine.Data.Core.Framework.Fields;";
		yield return "using MercuryEngine.Data.Core.Framework.Fields.Fluent;";
		yield return "using MercuryEngine.Data.Core.Framework.Structures;";
		yield return "using MercuryEngine.Data.Core.Framework.Structures.Fluent;";
		yield return "using MercuryEngine.Data.Types.Attributes;";
		yield return "using MercuryEngine.Data.Types.Extensions;";
		yield return "using MercuryEngine.Data.Types.Fields;";
	}
}