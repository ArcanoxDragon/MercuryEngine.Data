using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BtundaTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromPackages("btunda"))
			yield return new TestCaseData(testCase.Arguments[0], PackagesPath) { TestName = testCase.TestName };

		foreach (var testCase in GetTestCasesFromRomFs("btunda"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBtunda(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BTUNDA file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var btunda = new Btunda();

		try
		{
			btunda.Read(fileStream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(btunda, inFile, relativeTo);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBtunda(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Btunda>(inFile, relativeTo);
}