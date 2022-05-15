using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.SourceGenerators.Generators;

public abstract class BaseDreadGenerator<T> : IDreadGenerator
where T : IDreadType
{
	public string GenerateSource(IDreadType dreadType, GenerationContext context)
	{
		if (dreadType is not T derivedType)
			throw new InvalidOperationException($"{GetType().Name} only supports \"{typeof(T).FullName}\" Dread types");

		return GenerateSource(derivedType, context);
	}

	protected string GenerateSource(T dreadType, GenerationContext context)
		=> string.Join(Environment.NewLine, GenerateSourceLines(dreadType, context));

	protected abstract IEnumerable<string> GenerateSourceLines(T dreadType, GenerationContext context);
}