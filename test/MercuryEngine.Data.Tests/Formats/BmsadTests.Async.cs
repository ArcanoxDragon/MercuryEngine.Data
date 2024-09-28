using System.Diagnostics;
using MercuryEngine.Data.Formats;

namespace MercuryEngine.Data.Tests.Formats;

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
			DumpBmsadFile(bmsad, inFile, quiet);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public async Task TestCompareBmsadAsync(string inFile, bool quiet = false)
	{
		await ReadWriteAndCompareAsync<Bmsad>(inFile, PackagesPath, quiet, () => {
			if (inFile.EndsWith("pf_mushr_fr.bmsad"))
			{
				// Skip the comparison, but use Assert.Pass so there's a clear message
				Assert.Pass("The \"pf_mushr_fr.bmsad\" file has an encoding error in the base game, so comparison is skipped.");
				return false;
			}

			return true;
		});
	}
}