using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture, Parallelizable(ParallelScope.All)]
public class BcmdlTests : BaseTestFixture
{
	/// <summary>
	/// These 5 BCMDL files use a different format for the "UnknownMaterialParams" structure at the end.
	/// They also do not appear to be used by Dread (the game seems to work fine with them zeroed-out),
	/// so it's possible they are accidental leftovers from Metroid: Samus Returns.
	/// </summary>
	internal static readonly HashSet<string> FilesToSkip = [
		"system/engine/models/immune.bcmdl",
		"system/engine/models/sedisolve.bcmdl",
		"system/engine/models/sedisolver.bcmdl",
		"system/engine/models/selected_hi.bcmdl",
		"system/engine/models/selected_lo.bcmdl",
	];

	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bcmdl");

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBcmdl(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		if (FilesToSkip.Contains(fileName))
		{
			Assert.Ignore("Model with invalid UnknownMaterialParams data is ignored");
			return;
		}

		TestContext.Progress.WriteLine("Loading BCMDL file: {0}", fileName);

		var stream = OpenPackageFile(packageFilePath, packageFile);
		var bcmdl = new Bcmdl();

		try
		{
			bcmdl.Read(stream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bcmdl, fileName, print: false);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBcmdl(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		if (FilesToSkip.Contains(fileName))
		{
			Assert.Ignore("Model with invalid UnknownMaterialParams data is ignored");
			return;
		}

		TestContext.Progress.WriteLine("Loading BCMDL file: {0}", fileName);

		ReadWriteAndCompare<Bcmdl>(packageFilePath, packageFile, quiet: true);
	}
}