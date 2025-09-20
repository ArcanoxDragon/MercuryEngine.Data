using System.Diagnostics;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

public partial class BmsadTests
{
	[Test]
	public async Task TestDumpAllBmsadsAsync()
	{
		var bmsadTestCases = GetTestCasesFromPackages("bmsad").ToList();
		var sw = new Stopwatch();

		await TestContext.Progress.WriteAsync($"Dumping {bmsadTestCases.Count} BMSAD files...");
		sw.Start();

		foreach (var testCase in bmsadTestCases)
		{
			await TestLoadBmsadAsync((string) testCase.Arguments[0]!, (PackageFile) testCase.Arguments[1]!, quiet: true);
		}

		sw.Stop();
		await TestContext.Progress.WriteLineAsync($"Done! Took {sw.Elapsed}.");
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public async Task TestLoadBmsadAsync(string packageFilePath, PackageFile packageFile, bool quiet = false)
	{
		var fileName = packageFile.Name.ToString();

		if (!quiet)
			await TestContext.Progress.WriteLineAsync($"Loading BMSAD file: {fileName}");

		await using var stream = OpenPackageFile(packageFilePath, packageFile);
		var bmsad = new Bmsad();

		try
		{
			await bmsad.ReadAsync(stream);
		}
		finally
		{
			DumpBmsadFile(bmsad, fileName, quiet);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public async Task TestCompareBmsadAsync(string packageFilePath, PackageFile packageFile, bool quiet = false)
	{
		var fileName = packageFile.Name.ToString();

		await ReadWriteAndCompareAsync<Bmsad>(packageFilePath, packageFile, quiet: quiet, preCompareAction: () => {
			if (fileName.EndsWith("pf_mushr_fr.bmsad"))
			{
				// Skip the comparison, but use Assert.Pass so there's a clear message
				Assert.Pass("The \"pf_mushr_fr.bmsad\" file has an encoding error in the base game, so comparison is skipped.");
				return false;
			}

			return true;
		});
	}
}