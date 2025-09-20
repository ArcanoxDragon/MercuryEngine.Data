using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BtundaTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromPackages("btunda"))
		{
			var packageFile = (PackageFile) testCase.Arguments[1]!;
			var fileName = packageFile.Name.ToString();

			yield return new TestCaseData(fileName, null, testCase.Arguments[0], testCase.Arguments[1]) { TestName = testCase.TestName };
		}

		foreach (var testCase in GetTestCasesFromRomFs("btunda"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath, null, null) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBtunda(string fileName, string? relativeTo, string? packageFilePath, PackageFile? packageFile)
	{
		TestContext.Progress.WriteLine("Loading BTUNDA file: {0}", fileName);

		Stream stream;

		if (packageFile != null)
			stream = OpenPackageFile(packageFilePath!, packageFile);
		else
			stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

		var btunda = new Btunda();

		try
		{
			btunda.Read(stream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(btunda, fileName);
			stream.Dispose();
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBtunda(string fileName, string? relativeTo, string? packageFilePath, PackageFile? packageFile)
	{
		if (packageFile != null)
			ReadWriteAndCompare<Btunda>(packageFilePath!, packageFile);
		else
			ReadWriteAndCompare<Btunda>(fileName, relativeTo);
	}
}