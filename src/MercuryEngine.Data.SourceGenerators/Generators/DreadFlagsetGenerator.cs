using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public class DreadFlagsetGenerator : BaseDreadGenerator<DreadFlagsetType>
{
	public static DreadFlagsetGenerator Instance { get; } = new();

	protected override IEnumerable<string> GenerateSourceLines(DreadFlagsetType dreadType, GenerationContext context)
	{
		var typeName = dreadType.TypeName;
		var typeEnumName = TypeNameUtility.SanitizeTypeName(typeName)!;
		var enumTypeName = dreadType.Enum;

		if (enumTypeName is null)
			throw new InvalidOperationException($"Flagset type \"{typeName}\" is missing an enum name");

		if (!context.KnownTypes.TryGetValue(enumTypeName, out var enumBaseType))
			throw new InvalidOperationException($"Flagset type \"{typeName}\" has unknown enum type \"{enumTypeName}\"");

		if (enumBaseType is not DreadEnumType enumType)
			throw new InvalidOperationException($"Flagset type \"{typeName}\" referred to type \"{enumTypeName}\", which is not an enum type");

		yield return "[Flags]";
		yield return $"[DreadEnum(\"{typeName}\")]";
		yield return $"public enum {typeEnumName} : uint";
		yield return "{";

		foreach (var (name, value) in enumType.Values)
			yield return $"\t{name} = {value},";

		yield return "}";

		var csharpDataTypeName = $"DreadEnumDataType<{typeEnumName}>";

		context.GeneratedTypes.Add(new GeneratedType(csharpDataTypeName, typeName));
	}
}