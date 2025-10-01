using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class TocTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("toc"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadToc(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading TOC file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var toc = new Toc();

		try
		{
			toc.Read(fileStream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(toc, inFile, relativeTo);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareToc(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Toc>(inFile, relativeTo);
}