#if NET8_0_OR_GREATER
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Definitions.Utility;

[PublicAPI]
public static class KnownStrings
{
	static KnownStrings()
	{
		ParseStringsFromDataFile("DataDefinitions/dread_property_names.json");
		ParseStringsFromDataFile("DataDefinitions/dread_resource_names.json");
		ParseStringsFromDataFile("DataDefinitions/dread_discovered_strings.json");
	}

	public static IReadOnlyList<string> NewStrings => InternalKnownStrings.NewDiscoveredStrings;

	/// <summary>
	/// Gets a string by its CRC64 hash.
	/// </summary>
	public static string Get(ulong hash)
		=> InternalKnownStrings.Get(hash);

	/// <summary>
	/// Attempts to look up a string by its CRC64 hash.
	/// </summary>
	public static bool TryGet(ulong hash, [NotNullWhen(true)] out string? value)
		=> InternalKnownStrings.TryGet(hash, out value);

	/// <summary>
	/// Records the CRC64 hash of the provided <paramref name="value"/> so that it can be retrieved by the hash at a later point in time.
	/// </summary>
	public static void Record(string value)
		=> InternalKnownStrings.Record(value);

	private static void ParseStringsFromDataFile(string fileName)
	{
		const int BufferSize = 2048;

		using var fileStream = ResourceHelper.OpenResourceFile(fileName);
		var buffer = new byte[BufferSize];

		// Perform initial read from stream
		var bytesRead = fileStream.Read(buffer);

		// Set up reader
		var reader = new Utf8JsonReader(buffer.AsSpan(0, bytesRead), isFinalBlock: false, state: default);
		var insideObject = false;
		string? currentKey = null;

		while (reader.TokenType != JsonTokenType.EndObject)
		{
			while (!reader.Read())
				AdvanceStream(fileStream, ref buffer, ref reader);

			if (!insideObject)
			{
				if (reader.TokenType != JsonTokenType.StartObject)
					throw new JsonException($"Expected {JsonTokenType.StartObject} token while reading \"{fileName}\", but found {reader.TokenType} instead");

				insideObject = true;
				continue;
			}

			switch (reader.TokenType)
			{
				case JsonTokenType.PropertyName:
					currentKey = reader.GetString();
					break;
				case JsonTokenType.Number:
					if (currentKey is null)
						throw new JsonException($"Encountered a number before encountering a property key while reading \"{fileName}\" (at offset {reader.TokenStartIndex})");

					var crcHash = reader.GetUInt64();

					InternalKnownStrings.HashToStringLookup[crcHash] = currentKey;
					break;
				case JsonTokenType.EndObject:
					break;
				default:
					throw new JsonException($"Found unexpected {reader.TokenType} token while reading \"{fileName}\" (at offset {reader.TokenStartIndex})");
			}
		}
	}

	private static void AdvanceStream(Stream stream, ref byte[] buffer, ref Utf8JsonReader reader)
	{
		int bytesAvailable;

		if (reader.BytesConsumed < buffer.Length)
		{
			// Need to shift the unused portion of the buffer up to the start
			var remainingSpan = buffer.AsSpan((int) reader.BytesConsumed);

			if (remainingSpan.Length >= buffer.Length)
				Debugger.Break();

			Debug.Assert(remainingSpan.Length < buffer.Length, "Buffer not long enough for this JSON document!");

			remainingSpan.CopyTo(buffer);

			// Now fill the "right half" of the buffer with more data
			bytesAvailable = remainingSpan.Length + stream.Read(buffer.AsSpan(remainingSpan.Length));
		}
		else
		{
			// Consumed the entire buffer in the previous read operation, so we need to fill the whole thing again
			bytesAvailable = stream.Read(buffer);
		}

		reader = new Utf8JsonReader(buffer.AsSpan(0, bytesAvailable), isFinalBlock: bytesAvailable == 0, reader.CurrentState);
	}
}
#endif