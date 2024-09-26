using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Utility.Json;
using MercuryEngine.Data.Tests.Utility;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MercuryEngine.Data.Tests;

public partial class BmsadTests
{
	[Test]
	public async Task TestDumpAllBmsadsAsync()
	{
		var files = Directory.EnumerateFiles(PackagesPath, "*.bmsad", SearchOption.AllDirectories).ToList();
		var sw = new Stopwatch();

		await TestContext.Progress.WriteAsync($"Dumping {files.Count} BMSAD files...");
		sw.Start();

		foreach (var file in files)
			await TestLoadBmsadAsync(file, quiet: true);

		sw.Stop();
		await TestContext.Progress.WriteLineAsync($"Done! Took {sw.Elapsed}.");
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public async Task TestLoadBmsadAsync(string inFile, bool quiet = false)
	{
		if (!quiet)
			await TestContext.Progress.WriteLineAsync($"Loading BMSAD file: {inFile}");

		await using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bmsad = new Bmsad();

		try
		{
			await bmsad.ReadAsync(fileStream);
		}
		finally
		{
			await DumpBmsadFileAsync(bmsad, inFile, quiet);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public async Task TestCompareBmsadAsync(string inFile, bool quiet = false)
	{
		if (!quiet)
			TestContext.Progress.WriteLine("Parsing BMSAD file: {0}", inFile);

		await using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var dataMapper = new DataMapper();
		var bmsad = new Bmsad {
			DataMapper = dataMapper,
		};

		await bmsad.ReadAsync(fileStream);

		using var tempStream = new MemoryStream();

		await bmsad.WriteAsync(tempStream);

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, inFile))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSAD", relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(inFile) + "_out" + Path.GetExtension(inFile);
		var outFilePath = Path.Combine(outFileDir, outFileName);

		if (!quiet)
			await TestContext.Progress.WriteLineAsync($"Output BMSAD file: {outFilePath}");

		DataUtilities.DumpDataMapper(dataMapper, outFilePath);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		await tempStream.CopyToAsync(outFileStream);

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

				await TestContext.Error.WriteLineAsync("The pending data assertion failed at the following range in the written data:");
				await TestContext.Error.WriteLineAsync(failureRangePath.FormatRangePath());

				throw;
			}
		}
	}

	private static async Task DumpBmsadFileAsync(Bmsad bmsad, string bmsadFilePath, bool quiet = false)
	{
		var jsonDump = JsonSerializer.Serialize(bmsad, JsonUtility.JsonOptions);

		if (!quiet)
		{
			await TestContext.Out.WriteLineAsync("JSON dump of current parsed state:");
			await TestContext.Out.WriteLineAsync(jsonDump);
		}

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, bmsadFilePath))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSAD", relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(bmsadFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		await using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		await writer.WriteAsync(jsonDump);
	}
}