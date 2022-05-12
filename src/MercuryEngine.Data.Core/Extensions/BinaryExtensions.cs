namespace MercuryEngine.Data.Core.Extensions;

public static class BinaryExtensions
{
	public static string ToHexString(this IEnumerable<byte> data)
		=> string.Join(" ", data.Select(b => $"{b:x2}"));
}