using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BmscpTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bmscp"))
		{
			bool shouldSkip = testCase.TestName?.Contains("bossrushreport.bmscp", StringComparison.OrdinalIgnoreCase) == true ||
							  testCase.TestName?.Contains("popupgeneric.bmscp", StringComparison.OrdinalIgnoreCase) == true;
			TestCaseData newTestCase = new TestCaseData(testCase.Arguments[0], RomFsPath) {
				TestName = testCase.TestName,
				RunState = shouldSkip ? RunState.Skipped : RunState.Runnable,
			};

			if (shouldSkip)
				newTestCase.Properties.Set(PropertyNames.SkipReason, "This file cannot be read due to missing 2.1.0 type information");

			yield return newTestCase;
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBmscp(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BMSCP file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bmscp = new Bmscp();

		try
		{
			bmscp.Read(fileStream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bmscp, inFile, relativeTo);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBmscp(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Bmscp>(inFile, relativeTo);
}