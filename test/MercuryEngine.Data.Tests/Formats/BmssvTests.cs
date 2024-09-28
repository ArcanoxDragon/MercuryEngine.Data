using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public partial class BmssvTests : BaseTestFixture
{
	private const string TestFreshProfile = "Fresh";
	private const string TestHundoProfile = "Hundo";
	private const string TestRandoProfile = "Rando";

	private static readonly string BaseDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSSV");

	private static IEnumerable<string> GetTestFiles()
	{
		foreach (var profileDirectory in new[] { TestFreshProfile, TestHundoProfile, TestRandoProfile }.Select(GetTestProfilePath))
		foreach (var file in Directory.EnumerateFiles(profileDirectory, "*.bmssv", SearchOption.AllDirectories))
		{
			if (Path.GetFileNameWithoutExtension(file).EndsWith("_out", StringComparison.OrdinalIgnoreCase))
				continue;

			yield return file;
		}
	}

	private static string GetTestProfilePath(string profileName)
		=> Path.Combine(BaseDirectory, profileName);

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBmssv(string inFile)
	{
		var sourceFilePath = GetTestProfilePath(inFile);
		using var fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read);
		var bmssv = new Bmssv();

		try
		{
			bmssv.Read(fileStream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(bmssv, sourceFilePath, BaseDirectory);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBmssv(string inFile)
	{
		var sourceFilePath = GetTestProfilePath(inFile);

		ReadWriteAndCompare<Bmssv>(sourceFilePath, BaseDirectory);
	}
}