using MercuryEngine.Data.Definitions.DreadTypes;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public abstract class BaseDreadGenerator<T> : IDreadGenerator
where T : IDreadType
{
	public string GenerateSource(IDreadType dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
	{
		if (dreadType is not T derivedType)
			throw new InvalidOperationException($"{GetType().Name} only supports \"{typeof(T).FullName}\" Dread types");

		return GenerateSource(derivedType, executionContext, generationContext);
	}

	protected string GenerateSource(T dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext)
		=> string.Join("\n", GenerateSourceLines(dreadType, executionContext, generationContext));

	protected abstract IEnumerable<string> GenerateSourceLines(T dreadType, GeneratorExecutionContext executionContext, GenerationContext generationContext);
}