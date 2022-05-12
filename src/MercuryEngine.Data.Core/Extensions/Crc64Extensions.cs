using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Extensions;

[PublicAPI]
public static class Crc64Extensions
{
	public static ulong GetCrc64(this byte[] data)
		=> Crc64.Calculate(data);

	public static ulong GetCrc64(this string text)
		=> Crc64.Calculate(text);
}