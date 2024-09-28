using MercuryEngine.Data.Core.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace MercuryEngine.Data.Core.Utility;

internal class InternalKnownStrings
{
	public static readonly Dictionary<ulong, string> HashToStringLookup   = [];
	public static readonly List<string>              NewDiscoveredStrings = [];

	/// <summary>
	/// Gets a string by its CRC64 hash.
	/// </summary>
	public static string Get(ulong hash)
	{
		if (!TryGet(hash, out var value))
			throw new KeyNotFoundException($"There is no known string with a CRC hash of \"{hash.ToHexString()}\" (raw: {hash})");

		return value;
	}

	/// <summary>
	/// Attempts to look up a string by its CRC64 hash.
	/// </summary>
	public static bool TryGet(ulong hash, [NotNullWhen(true)] out string? value)
		=> HashToStringLookup.TryGetValue(hash, out value);

	/// <summary>
	/// Records the CRC64 hash of the provided <paramref name="value"/> so that it can be retrieved by the hash at a later point in time.
	/// </summary>
	public static void Record(string value)
	{
		var hash = value.GetCrc64();

		if (HashToStringLookup.TryAdd(hash, value))
		{
			NewDiscoveredStrings.Add(value);
			Debug.WriteLine("################ DISCOVERED NEW STRING!!! ################");
			Debug.WriteLine(value);
		}
	}
}