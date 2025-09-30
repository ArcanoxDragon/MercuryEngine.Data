using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BshdatTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bshdat"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBshdat(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BSHDAT file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bshdat = new Bshdat();

		try
		{
			bshdat.Read(fileStream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bshdat, inFile, relativeTo, print: false);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBshdat(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BSHDAT file: {0}", inFile);

		ReadWriteAndCompare<Bshdat>(inFile, relativeTo, quiet: true);
	}
}