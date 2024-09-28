namespace MercuryEngine.Data.SourceGenerators.Generators.Structs;

internal sealed record InvalidStructField(string FieldName, string ErrorMessage) : IStructField
{
	public bool ShouldGenerate => true;
	public bool HasSummary     => false;

	public string GenerateSummary()
		=> string.Empty;

	public IEnumerable<string> GenerateProperty()
		=> [$"// Error generating field \"{FieldName}\": {ErrorMessage}"];

	public string GenerateDefine()
		=> $"throw new NotSupportedException(\"Cannot read or write field \\\"{FieldName}\\\".\");";
}