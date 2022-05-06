using JetBrains.Annotations;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Extensions;

[PublicAPI]
public static class Crc64Extensions
{
	public static ulong GetCrc64(this IEnumerable<byte> data)
		=> Crc64.Calculate(data);

	public static ulong GetCrc64(this string text)
		=> Crc64.Calculate(text);
}