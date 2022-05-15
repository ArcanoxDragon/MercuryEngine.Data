using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public interface IDreadGenerator
{
	string GenerateSource(IDreadType dreadType, GenerationContext context);
}