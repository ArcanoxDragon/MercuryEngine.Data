using System.Text;

namespace MercuryEngine.Data.Core.Extensions;

public static class BinaryExtensions
{
	public static string ToHexString(this ulong value)
	{
		var bytes = BitConverter.GetBytes(value);

		if (BitConverter.IsLittleEndian)
			Array.Reverse(bytes);

		ReadOnlySpan<byte> span = bytes.AsSpan();

		return span.ToHexString();
	}

	public static string ToHexString(this ReadOnlySpan<byte> data)
	{
		var builder = new StringBuilder();
		var first = true;

		foreach (var b in data)
		{
			if (!first)
				builder.Append(' ');

			builder.Append(b.ToString("x2"));
			first = false;
		}

		return builder.ToString();
	}
}