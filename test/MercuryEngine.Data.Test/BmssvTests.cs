using System.IO;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Test.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MercuryEngine.Data.Test;

[TestFixture]
public class BmssvTests
{
	private const string TestFreshProfile    = "samus_fresh.bmssv";
	private const string TestFreshProfileOut = "samus_fresh_out.bmssv";

	private const string TestHundoProfile    = "samus_hundo.bmssv";
	private const string TestHundoProfileOut = "samus_hundo_out.bmssv";

	[TestCase(TestFreshProfile)]
	[TestCase(TestHundoProfile)]
	public void TestLoadSamusBmssv(string inFile)
	{
		var filePath = GetTestFilePath(inFile);
		using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		var bmssv = new Bmssv();

		try
		{
			bmssv.Read(fileStream);
		}
		finally
		{
			var jsonDump = JsonConvert.SerializeObject(bmssv, new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Converters = {
					new StringEnumConverter(),
					new TerminatedStringConverter(),
				},
			});

			TestContext.Out.WriteLine("JSON dump of current parsed state:");
			TestContext.Out.WriteLine(jsonDump);
		}
	}

	[TestCase(TestFreshProfile, TestFreshProfileOut)]
	[TestCase(TestHundoProfile, TestHundoProfileOut)]
	public void TestCompareSamusBmssv(string inFile, string outFile)
	{
		var filePath = GetTestFilePath(inFile);
		using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var bmssv = new Bmssv();

		bmssv.Read(fileStream);

		using var tempStream = new MemoryStream();

		bmssv.Write(tempStream);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		var outFilePath = GetTestFilePath(outFile);
		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		tempStream.CopyTo(outFileStream);

		Assert.That(newBuffer, Is.EquivalentTo(originalBuffer));
	}

	private string GetTestFilePath(string fileName)
		=> Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSSV", fileName);
}