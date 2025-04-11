using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.SourceGenerators;

internal record GeneratedType(BaseDreadType DreadType, string CSharpTypeName, string DreadTypeName, string? ParentTypeName = null);