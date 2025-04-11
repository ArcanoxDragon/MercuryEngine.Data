using System.Collections.Immutable;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.SourceGenerators;

internal sealed record GenerationContext(
	IReadOnlyDictionary<string, BaseDreadType> KnownTypes,
	ImmutableArray<PreexistingType> PreexistingTypes);