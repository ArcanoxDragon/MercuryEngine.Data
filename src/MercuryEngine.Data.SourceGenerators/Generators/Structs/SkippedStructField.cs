namespace MercuryEngine.Data.SourceGenerators.Generators.Structs;

internal sealed record SkippedStructField(string FieldName, string Reason) : IStructField
{
	public bool ShouldGenerate => true;
	public bool HasSummary     => false;

	public string GenerateSummary()
		=> string.Empty;

	public IEnumerable<string> GenerateProperty()
		=> [$"// Field \"{FieldName}\" was skipped because {Reason}"];

	public string GenerateDefine()
		=> string.Empty;
}