using System.Diagnostics;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public partial class BmsadTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		// Add "quiet" arg to test cases (NUnit can't handle optional parameters???)
		=> GetTestCasesFromPackages("bmsad").Select(tc => new TestCaseData(tc.Arguments[0], tc.Arguments[1], false) {
			TestName = tc.TestName,
		});

	[Test]
	public void TestDumpAllBmsads()
	{
		var bmsadTestCases = GetTestCasesFromPackages("bmsad").ToList();
		var sw = new Stopwatch();

		TestContext.Progress.Write($"Dumping {bmsadTestCases.Count} BMSAD files...");
		sw.Start();

		foreach (var testCase in bmsadTestCases)
			TestLoadBmsad((string) testCase.Arguments[0]!, (PackageFile) testCase.Arguments[1]!, quiet: true);

		sw.Stop();
		TestContext.Progress.WriteLine($"Done! Took {sw.Elapsed}.");
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBmsad(string packageFilePath, PackageFile packageFile, bool quiet = false)
	{
		var fileName = packageFile.Name.ToString();

		if (!quiet)
			TestContext.Progress.WriteLine("Loading BMSAD file: {0}", fileName);

		var bmsad = new Bmsad();
		using var stream = OpenPackageFile(packageFilePath, packageFile, bmsad.DisplayName);

		try
		{
			bmsad.Read(stream);
		}
		finally
		{
			DumpBmsadFile(bmsad, fileName, quiet);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBmsad(string packageFilePath, PackageFile packageFile, bool quiet = false)
	{
		var fileName = packageFile.Name.ToString();

		ReadWriteAndCompare<Bmsad>(packageFilePath, packageFile, quiet: quiet, preCompareAction: () => {
			if (fileName.EndsWith("pf_mushr_fr.bmsad"))
			{
				// Skip the comparison, but use Assert.Pass so there's a clear message
				Assert.Pass("The \"pf_mushr_fr.bmsad\" file has an encoding error in the base game, so comparison is skipped.");
				return false;
			}

			return true;
		});
	}

	private static void DumpBmsadFile(Bmsad bmsad, string bmsadFilePath, bool quiet = false)
		=> DataUtilities.DumpDataStructure(bmsad, bmsadFilePath, print: !quiet);
}