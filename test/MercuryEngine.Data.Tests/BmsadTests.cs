using System.Diagnostics;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests;

[TestFixture]
public partial class BmsadTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bmsad");

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
		ReadWriteAndCompare<Bmsad>(inFile, PackagesPath, quiet, () => {
			if (inFile.EndsWith("pf_mushr_fr.bmsad"))
			{
				// Skip the comparison, but use Assert.Pass so there's a clear message
				Assert.Pass("The \"pf_mushr_fr.bmsad\" file has an encoding error in the base game, so comparison is skipped.");
				return false;
			}

			return true;
		});
	}

	private static void DumpBmsadFile(Bmsad bmsad, string bmsadFilePath, bool quiet = false)
	{
		var jsonDump = DataUtilities.DumpDataStructure(bmsad, bmsadFilePath, PackagesPath);

		if (!quiet)
		{
			TestContext.Out.WriteLine("JSON dump of current parsed state:");
			TestContext.Out.WriteLine(jsonDump);
		}
	}
}