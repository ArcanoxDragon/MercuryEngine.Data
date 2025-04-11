using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

internal class DreadEnumGenerator : BaseDreadGenerator<DreadEnumType>
{
	public static DreadEnumGenerator Instance { get; } = new();

	public override IEnumerable<GeneratedType> GetTypesToGenerate(IReadOnlyDictionary<string, BaseDreadType> dreadTypes)
	{
		foreach (var (typeName, type) in dreadTypes)
		{
			if (Constants.ExcludedTypeNames.Contains(typeName) || type is not DreadEnumType)
				continue;

			var typeEnumName = TypeNameUtility.SanitizeTypeName(typeName);
			var csharpDataTypeName = $"DreadEnum<{typeEnumName}>";

			yield return new GeneratedType(type, csharpDataTypeName, typeName);
		}
	}

	protected override IEnumerable<string> GenerateSourceLines(GeneratedType generatedType, DreadEnumType dreadType, SourceProductionContext productionContext, GenerationContext generationContext)
	{
		var typeName = generatedType.DreadTypeName;
		var typeEnumName = TypeNameUtility.SanitizeTypeName(typeName);

		yield return $"[DreadEnum(\"{typeName}\")]";
		yield return $"public enum {typeEnumName} : uint";
		yield return "{";

		foreach (var (name, value) in dreadType.Values)
		{
			productionContext.CancellationToken.ThrowIfCancellationRequested();
			yield return $"\t{name} = {value},";
		}

		yield return "}";
	}
}