using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture, Parallelizable(ParallelScope.All)]
public class PkgTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("pkg"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadPkg(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading PKG file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var pkg = new Pkg();

		try
		{
			pkg.Read(fileStream);

			foreach (var file in pkg.Files)
				TestContext.Progress.WriteLine($"File: {file.Name} ({file.Length} bytes)");
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(pkg, inFile, relativeTo);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestComparePkg(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Pkg>(inFile, relativeTo);
}