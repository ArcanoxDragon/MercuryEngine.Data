using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

internal class DreadFlagsetGenerator : BaseDreadGenerator<DreadFlagsetType>
{
	public static DreadFlagsetGenerator Instance { get; } = new();

	public override IEnumerable<GeneratedType> GetTypesToGenerate(IReadOnlyDictionary<string, BaseDreadType> dreadTypes)
	{
		foreach (var (typeName, type) in dreadTypes)
		{
			if (Constants.ExcludedTypeNames.Contains(typeName) || type is not DreadFlagsetType)
				continue;

			var typeEnumName = TypeNameUtility.SanitizeTypeName(typeName);
			var csharpDataTypeName = $"DreadEnum<{typeEnumName}>";

			yield return new GeneratedType(type, csharpDataTypeName, typeName);
		}
	}

	protected override IEnumerable<string> GenerateSourceLines(GeneratedType generatedType, DreadFlagsetType dreadType, SourceProductionContext productionContext, GenerationContext generationContext)
	{
		var typeName = generatedType.DreadTypeName;
		var typeEnumName = TypeNameUtility.SanitizeTypeName(typeName);
		var enumTypeName = dreadType.Enum;

		if (enumTypeName is null)
			throw new InvalidOperationException($"Flagset type \"{typeName}\" is missing an enum name");

		if (!generationContext.KnownTypes.TryGetValue(enumTypeName, out var enumBaseType))
			throw new InvalidOperationException($"Flagset type \"{typeName}\" has unknown enum type \"{enumTypeName}\"");

		if (enumBaseType is not DreadEnumType enumType)
			throw new InvalidOperationException($"Flagset type \"{typeName}\" referred to type \"{enumTypeName}\", which is not an enum type");

		yield return "[Flags]";
		yield return $"[DreadEnum(\"{typeName}\")]";
		yield return $"public enum {typeEnumName} : uint";
		yield return "{";

		foreach (var (name, value) in enumType.Values)
		{
			productionContext.CancellationToken.ThrowIfCancellationRequested();
			yield return $"\t{name} = {value},";
		}

		yield return "}";
	}
}