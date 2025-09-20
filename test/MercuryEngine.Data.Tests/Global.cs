using System.Collections.Immutable;
using System.Text.Encodings.Web;
using System.Text.Json;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Definitions.Utility;

[assembly: LevelOfParallelism(16)]

namespace MercuryEngine.Data.Tests;

[SetUpFixture]
public class Global
{
	private static readonly JsonSerializerOptions NewStringsJsonOptions = new() {
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	[OneTimeSetUp]
	public void InitKnownStrings()
	{
		// We need to load the "KnownStrings" class before any tests are run so that
		// the JSON string dictionaries get parsed before any string fields are read
		KnownStrings.Record(string.Empty);
	}

	[OneTimeTearDown]
	public void WriteNewStrings()
	{
		if (KnownStrings.NewStrings.Count == 0)
			return;

		var stringToHashMap = KnownStrings.NewStrings.ToImmutableSortedDictionary(s => s, s => s.GetCrc64(), StringComparer.Ordinal);
		var stringToHashMapHex = KnownStrings.NewStrings.ToImmutableSortedDictionary(s => s, s => s.GetCrc64().ToHexString(), StringComparer.Ordinal);

		DumpDictionary(stringToHashMap, "dread_discovered_strings.json");
		DumpDictionary(stringToHashMapHex, "dread_discovered_strings_hex.json");

		static void DumpDictionary<TKey, TValue>(ImmutableSortedDictionary<TKey, TValue> dict, string fileName)
		where TKey : notnull
		{
			var newStringsJson = JsonSerializer.Serialize(dict, NewStringsJsonOptions);

			var outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
			var outputPath = Path.Combine(outputDir, fileName);

			Directory.CreateDirectory(outputDir);
			File.WriteAllText(outputPath, newStringsJson);
		}
	}
}