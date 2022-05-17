using System.Text;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.Definitions.Utility;
using MercuryEngine.Data.SourceGenerators.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MercuryEngine.Data.SourceGenerators;

[Generator]
public class DreadTypesGenerator : ISourceGenerator
{
	private static readonly DiagnosticDescriptor ErrorDescriptor = new(Guid.NewGuid().ToString(), "Generator Error", "Error: {0}", "DreadTypesGenerator", DiagnosticSeverity.Error, true);

	private const string DreadTypesNamespace        = "MercuryEngine.Data.Types.DreadTypes";
	private const string DreadTypeRegistryNamespace = "MercuryEngine.Data.Types";
	private const string DreadTypeRegistryClassName = "DreadTypeRegistry";

	private static readonly Dictionary<Type, IDreadGenerator> Generators = new() {
		{ typeof(DreadEnumType), DreadEnumGenerator.Instance },
		{ typeof(DreadFlagsetType), DreadFlagsetGenerator.Instance },
		{ typeof(DreadStructType), DreadStructGenerator.Instance },
	};

	private static readonly List<string> ExcludedTypeNames = new() { "char", "double", "long" };

	public void Initialize(GeneratorInitializationContext context) { }

	public void Execute(GeneratorExecutionContext executionContext)
	{
		var allTypes = DreadTypeParser.ParseDreadTypes();
		var generationContext = new GenerationContext(allTypes);

		GenerateTypesFile(executionContext, generationContext);
		GenerateRegistryPartialFile(executionContext, generationContext);
	}

	private void GenerateTypesFile(GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var sourceLines = string.Join(Environment.NewLine, GenerateTypesSourceLines(generationContext));
		var sourceText = SourceText.From(sourceLines, Encoding.UTF8);

		executionContext.AddSource("GeneratedDreadTypes.g.cs", sourceText);
	}

	private IEnumerable<string> GenerateTypesSourceLines(GenerationContext context)
	{
		yield return "#nullable enable";

		foreach (var line in GetStandardNamespaceImports())
			yield return line;

		yield return $"namespace {DreadTypesNamespace};";

		foreach (var (typeName, type) in context.KnownTypes)
		{
			if (ExcludedTypeNames.Contains(typeName))
				continue;

			if (!Generators.TryGetValue(type.GetType(), out var generator))
				continue;

			string source;
			bool isError = false;

			try
			{
				source = generator.GenerateSource(type, context);
			}
			catch (Exception ex)
			{
				var stackTraceLines = ex.ToString().Split('\n').Select(line => $"// {line}");

				source = $"// ERROR: Exception while generating type \"{typeName}\": {string.Join("\n", stackTraceLines)}";
				isError = true;
			}

			if (isError)
			{
				yield return source;
			}
			else
			{
				// XmlDoc comments
				yield return "/// <summary>";
				yield return $"/// Original type name: {typeName}&#10;";
				yield return $"/// Kind: {type.Kind}";
				yield return "/// </summary>";

				// Actual type source
				yield return source;
			}
		}
	}

	private void GenerateRegistryPartialFile(GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var sourceLines = string.Join(Environment.NewLine, GenerateRegistryPartialSourceLines(generationContext));
		var sourceText = SourceText.From(sourceLines, Encoding.UTF8);

		executionContext.AddSource("DreadTypeRegistry.g.cs", sourceText);
	}

	private IEnumerable<string> GenerateRegistryPartialSourceLines(GenerationContext context)
	{
		yield return "#nullable enable";

		foreach (var line in GetStandardNamespaceImports())
			yield return line;

		yield return $"using {DreadTypesNamespace};";

		yield return $"namespace {DreadTypeRegistryNamespace};";

		yield return $"public static partial class {DreadTypeRegistryClassName}";
		yield return "{";

		yield return "\tstatic partial void RegisterGeneratedTypes()";
		yield return "\t{";

		foreach (var (csharpTypeName, dreadTypeName) in context.GeneratedTypes)
			yield return $"\t\tRegisterConcreteType<{csharpTypeName}>(\"{dreadTypeName}\");";

		yield return "\t}";

		yield return "}";
	}

	private IEnumerable<string> GetStandardNamespaceImports()
	{
		yield return "using System;";
		yield return "using MercuryEngine.Data.Core.Framework;";
		yield return "using MercuryEngine.Data.Core.Framework.DataTypes;";
		yield return "using MercuryEngine.Data.Core.Framework.Structures;";
		yield return "using MercuryEngine.Data.Core.Framework.Structures.Fluent;";
		yield return "using MercuryEngine.Data.Types.Attributes;";
		yield return "using MercuryEngine.Data.Types.DataTypes;";
		yield return "using MercuryEngine.Data.Types.Extensions;";
	}
}