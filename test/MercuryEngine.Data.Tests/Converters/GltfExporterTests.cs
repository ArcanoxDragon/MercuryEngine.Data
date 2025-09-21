using MercuryEngine.Data.Converters.Bcmdl;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Converters;

[TestFixture]
public class GltfExporterTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
		=> GetTestCasesFromPackages("bcmdl");

	[TestCaseSource(nameof(GetTestFiles)), Explicit]
	public void TestExportGltf(string packageFilePath, PackageFile packageFile)
	{
		var fileName = packageFile.Name.ToString();

		if (BcmdlTests.FilesToSkip.Contains(fileName))
		{
			Assert.Ignore("Model with invalid UnknownMaterialParams data is ignored");
			return;
		}

		TestContext.Progress.WriteLine("Converting BCMDL file to GLB: {0}", fileName);

		Bcmdl bcmdl;

		using (var stream = OpenPackageFile(packageFilePath, packageFile))
		{
			bcmdl = new Bcmdl();
			bcmdl.Read(stream);
		}

		var relativeDirectory = Path.GetDirectoryName(fileName)!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", bcmdl.DisplayName, relativeDirectory);
		var outFileName = Path.GetFileNameWithoutExtension(fileName) + ".glb";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		var textureResolver = new TestMaterialResolver();
		var exporter = new GltfExporter(textureResolver);

		exporter.ExportGltf(bcmdl, outFilePath, sceneName: Path.GetFileNameWithoutExtension(fileName));
	}
}