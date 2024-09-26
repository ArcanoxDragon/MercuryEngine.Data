using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Tests.Utility.Json;

namespace MercuryEngine.Data.Tests;

[TestFixture]
public partial class BmsadTests
{
	private static readonly string PackagesPath = Configuration.PackagesPath; // Store it for faster access

	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var bmsadFile in Directory.EnumerateFiles(PackagesPath, "*.bmsad", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(PackagesPath, bmsadFile);

			yield return new TestCaseData(bmsadFile, false) {
				TestName = relativePath,
			};
		}
	}

	[Test]
	public void TestDumpAllBmsads()
	{
		var files = Directory.EnumerateFiles(PackagesPath, "*.bmsad", SearchOption.AllDirectories).ToList();
		var sw = new Stopwatch();

		TestContext.Progress.Write($"Dumping {files.Count} BMSAD files...");
		sw.Start();

		foreach (var file in files)
			TestLoadBmsad(file, quiet: true);

		sw.Stop();
		TestContext.Progress.WriteLine($"Done! Took {sw.Elapsed}.");
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBmsad(string inFile, bool quiet = false)
	{
		if (!quiet)
			TestContext.Progress.WriteLine("Loading BMSAD file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bmsad = new Bmsad();

		try
		{
			bmsad.Read(fileStream);
		}
		finally
		{
			DumpBmsadFile(bmsad, inFile, quiet);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBmsad(string inFile, bool quiet = false)
	{
		if (!quiet)
			TestContext.Progress.WriteLine("Parsing BMSAD file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var dataMapper = new DataMapper();
		var bmsad = new Bmsad {
			DataMapper = dataMapper,
		};

		bmsad.Read(fileStream);

		using var tempStream = new MemoryStream();

		bmsad.Write(tempStream);

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, inFile))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSAD", relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(inFile) + "_out" + Path.GetExtension(inFile);
		var outFilePath = Path.Combine(outFileDir, outFileName);

		if (!quiet)
			TestContext.Progress.WriteLine("Output BMSAD file: {0}", outFilePath);

		DataUtilities.DumpDataMapper(dataMapper, outFilePath);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		tempStream.CopyTo(outFileStream);

		if (inFile.EndsWith("pf_mushr_fr.bmsad"))
			Assert.Pass("The \"pf_mushr_fr.bmsad\" file has an encoding error in the base game, so comparison is skipped.");

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

	private static void DumpBmsadFile(Bmsad bmsad, string bmsadFilePath, bool quiet = false)
	{
		var jsonDump = JsonSerializer.Serialize(bmsad, JsonUtility.JsonOptions);

		if (!quiet)
		{
			TestContext.Out.WriteLine("JSON dump of current parsed state:");
			TestContext.Out.WriteLine(jsonDump);
		}

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, bmsadFilePath))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSAD", relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(bmsadFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		writer.Write(jsonDump);
	}
}