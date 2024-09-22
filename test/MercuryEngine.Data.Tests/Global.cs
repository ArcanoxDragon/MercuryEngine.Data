using System.Collections.Immutable;
using System.Text.Encodings.Web;
using System.Text.Json;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Definitions.Utility;

namespace MercuryEngine.Data.Tests;

[SetUpFixture]
public class Global
{
	private static readonly JsonSerializerOptions NewStringsJsonOptions = new() {
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	[OneTimeTearDown]
	public void WriteNewStrings()
	{
		if (KnownStrings.NewStrings.Count == 0)
			return;

		var stringToHashMap = KnownStrings.NewStrings.ToImmutableSortedDictionary(s => s, s => s.GetCrc64(), StringComparer.Ordinal);
		var newStringsJson = JsonSerializer.Serialize(stringToHashMap, NewStringsJsonOptions);

		var outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
		var outputPath = Path.Combine(outputDir, "discovered_strings.json");

		Directory.CreateDirectory(outputDir);
		File.WriteAllText(outputPath, newStringsJson);
	}
}