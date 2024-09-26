using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Core.Framework.Mapping;

public sealed class DataRange(string description, ulong start)
{
	[JsonPropertyOrder(1)]
	public string Description { get; } = description;

	[JsonPropertyOrder(2)]
	public ulong Start { get; } = start;

	[JsonPropertyOrder(3)]
	public ulong End { get; internal set; } = start;

	[JsonPropertyOrder(4)]
	public List<DataRange> InnerRanges { get; } = [];

	public override string ToString() => Description;
}