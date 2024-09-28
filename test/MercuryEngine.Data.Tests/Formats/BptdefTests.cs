using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BptdefTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bptdef");

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBptdef(string inFile)
	{
		TestContext.Progress.WriteLine("Loading BPTDEF file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bptdef = new Bptdef();

		try
		{
			bptdef.Read(fileStream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(bptdef, inFile, PackagesPath);
		}

		Assert.That(bptdef.CheckpointDefs, Is.Not.Null);

		// Write a CSV of all checkpoints
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, inFile))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", bptdef.DisplayName, relativePath);
		var csvFileName = Path.GetFileNameWithoutExtension(inFile) + ".csv";
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
	public void TestCompareBptdef(string inFile)
		=> ReadWriteAndCompare<Bptdef>(inFile, PackagesPath);
}