using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture, Parallelizable(ParallelScope.All)]
public class BcsklaTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bcskla");

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBcskla(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		TestContext.Progress.WriteLine("Loading BCSKLA file: {0}", fileName);

		var bcskla = new Bcskla();
		var stream = OpenPackageFile(packageFilePath, packageFile, bcskla.DisplayName);

		try
		{
			bcskla.Read(stream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bcskla, fileName, print: false);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBcskla(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		TestContext.Progress.WriteLine("Loading BCSKLA file: {0}", fileName);

		ReadWriteAndCompare<Bcskla>(packageFilePath, packageFile, quiet: true);
	}
}