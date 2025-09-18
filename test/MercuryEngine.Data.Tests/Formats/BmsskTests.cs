using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BmsskTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bmssk"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBmssk(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BMSSK file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bmssk = new Bmssk();

		try
		{
			bmssk.Read(fileStream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bmssk, inFile, relativeTo);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBmssk(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Bmssk>(inFile, relativeTo);
}