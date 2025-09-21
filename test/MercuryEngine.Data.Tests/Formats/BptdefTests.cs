using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BptdefTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bptdef");

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBptdef(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		TestContext.Progress.WriteLine("Loading BPTDEF file: {0}", fileName);

		var bptdef = new Bptdef();
		using var stream = OpenPackageFile(packageFilePath, packageFile, bptdef.DisplayName);

		try
		{
			bptdef.Read(stream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(bptdef, fileName);
		}

		Assert.That(bptdef.CheckpointDefs, Is.Not.Null);

		// Write a CSV of all checkpoints
		var relativePath = Path.GetDirectoryName(fileName)!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", bptdef.DisplayName, relativePath);
		var csvFileName = Path.GetFileNameWithoutExtension(fileName) + ".csv";
		var csvFilePath = Path.Combine(outFileDir, csvFileName);

		using var csvStream = File.Open(csvFilePath, FileMode.Create, FileAccess.Write);
		using var csvWriter = new StreamWriter(csvStream);

		csvWriter.WriteLine("Scenario Name,Checkpoint Name,Start Point,Description,Tags");

		foreach (var checkpoint in bptdef.CheckpointDefs)
		{
			if (checkpoint is null)
				continue;

			var tags = checkpoint.Tags?.Replace(',', '|');

			csvWriter.WriteLine($"{checkpoint.ScenarioID},{checkpoint.CheckpointID},{checkpoint.StartPoint},{checkpoint.Desc},{tags}");
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBptdef(string packageFilePath, PackageFile packageFile)
		=> ReadWriteAndCompare<Bptdef>(packageFilePath, packageFile);
}