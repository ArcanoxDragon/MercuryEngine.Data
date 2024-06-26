﻿using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public class DreadEnumGenerator : BaseDreadGenerator<DreadEnumType>
{
	public static DreadEnumGenerator Instance { get; } = new();

	protected override IEnumerable<string> GenerateSourceLines(DreadEnumType dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		var typeName = dreadType.TypeName;
		var typeEnumName = TypeNameUtility.SanitizeTypeName(typeName);

		yield return $"[DreadEnum(\"{typeName}\")]";
		yield return $"public enum {typeEnumName} : uint";
		yield return "{";

		foreach (var (name, value) in dreadType.Values)
			yield return $"\t{name} = {value},";

		yield return "}";

		var csharpDataTypeName = $"DreadEnum<{typeEnumName}>";

		generationContext.GeneratedTypes.Add(new GeneratedType(csharpDataTypeName, typeName));
	}
}