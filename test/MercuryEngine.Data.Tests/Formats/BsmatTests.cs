using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture, Parallelizable(ParallelScope.All)]
public class BsmatTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bsmat");

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBsmat(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		TestContext.Progress.WriteLine("Loading BSMAT file: {0}", fileName);

		var bsmat = new Bsmat();
		var stream = OpenPackageFile(packageFilePath, packageFile, bsmat.DisplayName);

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
	public void TestCompareBsmat(string packageFilePath, PackageFile packageFile)
		=> ReadWriteAndCompare<Bsmat>(packageFilePath, packageFile);
}