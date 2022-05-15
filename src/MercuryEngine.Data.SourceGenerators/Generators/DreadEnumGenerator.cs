using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.SourceGenerators.Utility;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public class DreadEnumGenerator : BaseDreadGenerator<DreadEnumType>
{
	public static DreadEnumGenerator Instance { get; } = new();

	protected override IEnumerable<string> GenerateSourceLines(DreadEnumType dreadType, GenerationContext context)
	{
		var typeName = TypeNameUtility.SanitizeTypeName(dreadType.TypeName);

		yield return $"public enum {typeName} : uint";
		yield return "{";

		foreach (var (name, value) in dreadType.Values)
			yield return $"\t{name} = {value},";

		yield return "}";
	}
}