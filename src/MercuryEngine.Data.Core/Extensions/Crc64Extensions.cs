using System.Collections.Concurrent;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Extensions;

[PublicAPI]
public static class Crc64Extensions
{
	private static readonly ConcurrentDictionary<string, ulong> CrcCache = [];

	public static ulong GetCrc64(this byte[] data)
		=> Crc64.Calculate(data);

	public static ulong GetCrc64(this string text)
		=> CrcCache.GetOrAdd(text, Crc64.Calculate);
}