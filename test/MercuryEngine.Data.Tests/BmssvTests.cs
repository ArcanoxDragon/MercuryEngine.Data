using System.Text;
using System.Text.Json;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Tests.Utility.Json;

namespace MercuryEngine.Data.Tests;

[TestFixture]
public partial class BmssvTests
{
	private const string TestFreshProfile = "Fresh";
	private const string TestHundoProfile = "Hundo";
	private const string TestRandoProfile = "Rando";

	private static IEnumerable<string> GetTestFiles()
	{
		foreach (var profileDirectory in new[] { TestFreshProfile, TestHundoProfile, TestRandoProfile }.Select(GetTestProfilePath))
		foreach (var file in Directory.EnumerateFiles(profileDirectory, "*.bmssv", SearchOption.AllDirectories))
		{
			if (Path.GetFileNameWithoutExtension(file).EndsWith("_out", StringComparison.OrdinalIgnoreCase))
				continue;

			yield return file;
		}
	}

	private static string GetTestProfilePath(string profileName)
		=> Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSSV", profileName);

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBmssv(string inFile)
	{
		var filePath = GetTestProfilePath(inFile);
		using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		var bmssv = new Bmssv();

		try
		{
			bmssv.Read(fileStream);
		}
		finally
		{
			DumpBmssvFile(bmssv, filePath);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBmssv(string inFile)
	{
		var filePath = GetTestProfilePath(inFile);

		TestContext.Progress.WriteLine("Parsing BMSSV file: {0}", filePath);

		using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var dataMapper = new DataMapper();
		var bmssv = new Bmssv {
			DataMapper = dataMapper,
		};

		bmssv.Read(fileStream);

		using var tempStream = new MemoryStream();

		bmssv.Write(tempStream);

		DataUtilities.DumpDataMapper(dataMapper, filePath);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		var outFileDir = Path.GetDirectoryName(filePath)!;
		var outFileName = Path.GetFileNameWithoutExtension(filePath) + "_out" + Path.GetExtension(filePath);
		var outFilePath = Path.Combine(outFileDir, outFileName);

		TestContext.Progress.WriteLine("Output BMSSV file: {0}", outFilePath);

		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		tempStream.CopyTo(outFileStream);

		Assert.That(newBuffer, Has.Length.EqualTo(originalBuffer.Length), "New data was a different length than the original data");

		for (var i = 0; i < newBuffer.Length; i++)
		{
			try
			{
				Assert.That(newBuffer[i], Is.EqualTo(originalBuffer[i]), $"Data mismatch at offset {i}");
			}
			catch (AssertionException)
			{
				var failureRangePath = dataMapper.GetContainingRanges((ulong) i);

				TestContext.Error.WriteLine("The pending data assertion failed at the following range in the written data:");
				TestContext.Error.WriteLine(failureRangePath.FormatRangePath());

				throw;
			}
		}
	}

	private static void DumpBmssvFile(Bmssv bmssv, string bmssvFilePath)
	{
		var jsonDump = JsonSerializer.Serialize(bmssv, JsonUtility.JsonOptions);

		TestContext.Out.WriteLine("JSON dump of current parsed state:");
		TestContext.Out.WriteLine(jsonDump);

		var outFileDir = Path.GetDirectoryName(bmssvFilePath)!;
		var outFileName = Path.GetFileNameWithoutExtension(bmssvFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);
		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		writer.Write(jsonDump);
	}
}