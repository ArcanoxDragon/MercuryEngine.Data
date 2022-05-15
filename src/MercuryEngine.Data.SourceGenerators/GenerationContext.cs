using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.SourceGenerators;

public record GenerationContext(IReadOnlyDictionary<string, BaseDreadType> KnownTypes);