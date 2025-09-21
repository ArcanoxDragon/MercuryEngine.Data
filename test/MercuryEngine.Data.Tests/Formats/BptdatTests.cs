using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BptdatTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bptdat");

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBptdat(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		TestContext.Progress.WriteLine("Loading BPTDAT file: {0}", fileName);

		var bptdat = new Bptdat();
		using var stream = OpenPackageFile(packageFilePath, packageFile, bptdat.DisplayName);

		try
		{
			bptdat.Read(stream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(bptdat, fileName);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBptdat(string packageFilePath, PackageFile packageFile)
		=> ReadWriteAndCompare<Bptdat>(packageFilePath, packageFile);
}