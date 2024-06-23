using System.Text;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.Definitions.Utility;
using MercuryEngine.Data.SourceGenerators.Generators;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MercuryEngine.Data.SourceGenerators;

[Generator]
public class DreadTypesGenerator : ISourceGenerator
{
	private static readonly Dictionary<Type, IDreadGenerator> Generators = new() {
		{ typeof(DreadEnumType), DreadEnumGenerator.Instance },
		{ typeof(DreadFlagsetType), DreadFlagsetGenerator.Instance },
		{ typeof(DreadStructType), DreadStructGenerator.Instance },
	};

	private static readonly List<string> ExcludedTypeNames = ["char", "double", "long"];

	public void Initialize(GeneratorInitializationContext context)
	{
		var allTypes = DreadTypeParser.ParseDreadTypes();

		context.RegisterForSyntaxNotifications(() => new GenerationContext(allTypes));
	}

	public void Execute(GeneratorExecutionContext executionContext)
	{
		var generationContext = (GenerationContext) executionContext.SyntaxContextReceiver!;

		GenerateTypesFile(executionContext, generationContext);
		GenerateRegistryPartialFile(executionContext, generationContext);
		GenerateTypeNamesMappingFile(executionContext, generationContext);
	}

	private void GenerateTypesFile(GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var sourceLines = string.Join("\n", GenerateTypesSourceLines(executionContext, generationContext));
		var sourceText = SourceText.From(sourceLines, Encoding.UTF8);

		executionContext.AddSource("GeneratedDreadTypes.g.cs", sourceText);
	}

	private IEnumerable<string> GenerateTypesSourceLines(GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		yield return "#nullable enable";

		foreach (var line in GetStandardNamespaceImports())
			yield return line;

		yield return $"namespace {Constants.DreadTypesNamespace};";

		foreach (var (typeName, type) in generationContext.KnownTypes)
		{
			if (ExcludedTypeNames.Contains(typeName))
				continue;

			if (!Generators.TryGetValue(type.GetType(), out var generator))
				continue;

			string source;
			bool isError = false;

			try
			{
				source = generator.GenerateSource(type, executionContext, generationContext);
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
				yield return $"/// Original type name: {TypeNameUtility.XmlEscapeTypeName(typeName)}&#10;";
				yield return $"/// Kind: {type.Kind}";
				yield return "/// </summary>";

				// Actual type source
				yield return source;
			}
		}
	}

	private void GenerateRegistryPartialFile(GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var sourceLines = string.Join("\n", GenerateRegistryPartialSourceLines(generationContext));
		var sourceText = SourceText.From(sourceLines, Encoding.UTF8);

		executionContext.AddSource("DreadTypeRegistry.g.cs", sourceText);
	}

	private IEnumerable<string> GenerateRegistryPartialSourceLines(GenerationContext context)
	{
		yield return "#nullable enable";

		foreach (var line in GetStandardNamespaceImports())
			yield return line;

		yield return $"using {Constants.DreadTypesNamespace};";

		yield return $"namespace {Constants.DreadTypeRegistryNamespace};";

		yield return $"public static partial class {Constants.DreadTypeRegistryClassName}";
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
		yield return "using MercuryEngine.Data.Core.Framework.Fields;";
		yield return "using MercuryEngine.Data.Core.Framework.Structures;";
		yield return "using MercuryEngine.Data.Core.Framework.Structures.Fluent;";
		yield return "using MercuryEngine.Data.Types.Attributes;";
		yield return "using MercuryEngine.Data.Types.Extensions;";
		yield return "using MercuryEngine.Data.Types.Fields;";
	}

	private void GenerateTypeNamesMappingFile(GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var sourceLines = string.Join("\n", GenerateTypeNamesMappingSourceLines(generationContext));
		var sourceText = SourceText.From(sourceLines, Encoding.UTF8);

		executionContext.AddSource("dread_type_names.g.json", sourceText);
	}

	private IEnumerable<string> GenerateTypeNamesMappingSourceLines(GenerationContext context)
	{
		yield return "/*";
		yield return "{";

		foreach (var (typeName, _) in context.KnownTypes)
		{
			yield return $"\"{typeName}\": {{";

			var crc64 = typeName.GetCrc64();
			var hex = crc64.ToHexString();

			yield return $"\t\"crc_ulong\": {crc64},";
			yield return $"\t\"crc_hex\": \"{hex}\"";

			yield return "}";
		}

		yield return "}";
		yield return "*/";
	}
}