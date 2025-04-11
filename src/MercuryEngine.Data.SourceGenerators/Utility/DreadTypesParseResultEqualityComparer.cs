namespace MercuryEngine.Data.SourceGenerators.Utility;

internal class DreadTypesParseResultEqualityComparer : IEqualityComparer<DreadTypesParseResult>
{
	public static DreadTypesParseResultEqualityComparer Instance { get; } = new();

	public bool Equals(DreadTypesParseResult x, DreadTypesParseResult y)
	{
		if (ReferenceEquals(x.SourceText, y.SourceText))
			return true;
		if (ReferenceEquals(x.SourceText, null))
			return false;
		if (ReferenceEquals(y.SourceText, null))
			return false;

		return x.SourceText.ContentEquals(y.SourceText);
	}

	public int GetHashCode(DreadTypesParseResult obj)
		=> obj.SourceText != null ? obj.SourceText.GetHashCode() : 0;
}