using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BcmdlTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromPackages("bcmdl"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable(ParallelScope.Self)]
	public void TestLoadBcmdl(string inFile, string relativeTo)
	{
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

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable(ParallelScope.Self)]
	public void TestCompareBcmdl(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Bcmdl>(inFile, relativeTo, quiet: true);
}