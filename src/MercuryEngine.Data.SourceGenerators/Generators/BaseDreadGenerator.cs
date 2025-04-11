using System.Text;
using MercuryEngine.Data.Definitions.DreadTypes;
using Microsoft.CodeAnalysis;

namespace MercuryEngine.Data.SourceGenerators.Generators;

internal abstract class BaseDreadGenerator<T> : IDreadGenerator
where T : IDreadType
{
	public abstract IEnumerable<GeneratedType> GetTypesToGenerate(IReadOnlyDictionary<string, BaseDreadType> dreadTypes);

	public string GenerateSource(GeneratedType generatedType, SourceProductionContext productionContext, GenerationContext generationContext)
	{
		if (generatedType.DreadType is not T derivedType)
			throw new InvalidOperationException($"{GetType().Name} only supports \"{typeof(T).FullName}\" Dread types");

		return GenerateSource(generatedType, derivedType, productionContext, generationContext);
	}

	protected string GenerateSource(GeneratedType generatedType, T dreadType, SourceProductionContext productionContext, GenerationContext generationContext)
	{
		var builder = new StringBuilder();

		foreach (var line in GenerateSourceLines(generatedType, dreadType, productionContext, generationContext))
		{
			productionContext.CancellationToken.ThrowIfCancellationRequested();
			builder.AppendLine(line);
		}

		return builder.ToString();
	}

	protected abstract IEnumerable<string> GenerateSourceLines(
		GeneratedType generatedType,
		T dreadType,
		SourceProductionContext productionContext,
		GenerationContext generationContext);
}