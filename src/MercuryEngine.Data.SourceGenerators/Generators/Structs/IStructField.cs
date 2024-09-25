namespace MercuryEngine.Data.SourceGenerators.Generators.Structs;

internal interface IStructField
{
	bool   ShouldGenerate { get; }
	bool   HasSummary     { get; }
	string FieldName      { get; }

	string GenerateSummary();
	IEnumerable<string> GenerateProperty();
	string GenerateDefine();
}