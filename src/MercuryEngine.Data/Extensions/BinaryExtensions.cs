namespace MercuryEngine.Data.Extensions;

internal static class BinaryExtensions
{
	public static string ToHexString(this IEnumerable<byte> data)
		=> string.Join(" ", data.Select(b => $"{b:x2}"));
}