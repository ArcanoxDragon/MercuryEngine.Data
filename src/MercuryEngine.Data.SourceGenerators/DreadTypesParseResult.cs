using MercuryEngine.Data.Definitions.DreadTypes;
using Microsoft.CodeAnalysis.Text;

namespace MercuryEngine.Data.SourceGenerators;

internal record struct DreadTypesParseResult(Dictionary<string, BaseDreadType> DreadTypes, SourceText? SourceText, Exception? Exception = null);