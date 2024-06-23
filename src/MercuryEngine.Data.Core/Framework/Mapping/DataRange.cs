namespace MercuryEngine.Data.Core.Framework.Mapping;

public sealed class DataRange(string description, ulong start)
{
	public ulong Start { get; }               = start;
	public ulong End   { get; internal set; } = start;

	public List<DataRange> InnerRanges { get; } = [];

	public string Description { get; } = description;

	public override string ToString() => Description;
}