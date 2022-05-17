using MercuryEngine.Data.Definitions.DreadTypes;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public interface IDreadGenerator
{
	string GenerateSource(IDreadType dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext);
}