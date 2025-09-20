using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture, Parallelizable(ParallelScope.All)]
public class BcmdlTests : BaseTestFixture
{
	/// <summary>
	/// These 5 BCMDL files use a different format for the "UnknownMaterialParams" structure at the end.
	/// They also do not appear to be used by Dread (the game seems to work fine with them zeroed-out),
	/// so it's possible they are accidental leftovers from Metroid: Samus Returns.
	/// </summary>
	private static readonly HashSet<string> BcmdlTestsToSkip = [
		"packs/system/system/system/engine/models/immune.bcmdl",
		"packs/system/system/system/engine/models/sedisolve.bcmdl",
		"packs/system/system/system/engine/models/sedisolver.bcmdl",
		"packs/system/system/system/engine/models/selected_hi.bcmdl",
		"packs/system/system/system/engine/models/selected_lo.bcmdl",
	];

	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromPackages("bcmdl"))
			yield return new TestCaseData(testCase.Arguments[0], PackagesPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBcmdl(string inFile, string relativeTo)
	{
		string normalizedPath = Path.GetRelativePath(relativeTo, inFile).Replace('\\', '/').Trim('/');

		if (BcmdlTestsToSkip.Contains(normalizedPath))
		{
			Assert.Ignore("Model with invalid UnknownMaterialParams data is ignored");
			return;
		}

		TestContext.Progress.WriteLine("Loading BCMDL file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bcmdl = new Bcmdl();

		try
		{
			bcmdl.Read(fileStream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bcmdl, inFile, relativeTo, print: false);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBcmdl(string inFile, string relativeTo)
	{
		string normalizedPath = Path.GetRelativePath(relativeTo, inFile).Replace('\\', '/').Trim('/');

		if (BcmdlTestsToSkip.Contains(normalizedPath))
		{
			Assert.Ignore("Model with invalid UnknownMaterialParams data is ignored");
			return;
		}

		TestContext.Progress.WriteLine("Loading BCMDL file: {0}", inFile);

		ReadWriteAndCompare<Bcmdl>(inFile, relativeTo, quiet: true);
	}
}