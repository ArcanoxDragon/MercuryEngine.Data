using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture, Parallelizable(ParallelScope.All)]
public class BsmatTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromPackages("bsmat"))
		{
			var packageFile = (PackageFile) testCase.Arguments[1]!;
			var fileName = packageFile.Name.ToString();

			yield return new TestCaseData(fileName, null, testCase.Arguments[0], testCase.Arguments[1]) { TestName = testCase.TestName };
		}

		foreach (var testCase in GetTestCasesFromRomFs("bsmat"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath, null, null) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBsmat(string fileName, string? relativeTo, string? packageFilePath, PackageFile? packageFile)
	{
		TestContext.Progress.WriteLine("Loading BSMAT file: {0}", fileName);

		var bsmat = new Bsmat();
		Stream stream;

		if (packageFile != null)
			stream = OpenPackageFile(packageFilePath!, packageFile, bsmat.DisplayName);
		else
			stream = OpenRomFsFile(fileName, relativeTo, bsmat.DisplayName);

		try
		{
			bsmat.Read(stream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bsmat, fileName);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBsmat(string fileName, string? relativeTo, string? packageFilePath, PackageFile? packageFile)
	{
		if (packageFile != null)
			ReadWriteAndCompare<Bsmat>(packageFilePath!, packageFile);
		else
			ReadWriteAndCompare<Bsmat>(fileName, relativeTo);
	}
}