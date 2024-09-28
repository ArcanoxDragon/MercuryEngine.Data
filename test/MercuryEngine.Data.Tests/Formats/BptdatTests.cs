using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BptdatTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bptdat");

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBptdat(string inFile)
	{
		TestContext.Progress.WriteLine("Loading BPTDAT file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bptdat = new Bptdat();

		try
		{
			bptdat.Read(fileStream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(bptdat, inFile, PackagesPath);
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBptdat(string inFile)
		=> ReadWriteAndCompare<Bptdat>(inFile, PackagesPath);
}