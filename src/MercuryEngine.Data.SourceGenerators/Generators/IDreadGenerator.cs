using MercuryEngine.Data.Definitions.DreadTypes;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

internal interface IDreadGenerator
{
	IEnumerable<GeneratedType> GetTypesToGenerate(IReadOnlyDictionary<string, BaseDreadType> dreadTypes);
	string GenerateSource(GeneratedType generatedType, SourceProductionContext productionContext, GenerationContext generationContext);
}