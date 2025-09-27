using SkiaSharp;
using MercuryEngine.Data.TegraTextureLib.Extensions;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.ImageProcessing;
using BaseMedTestFixture = MercuryEngine.Data.Tests.Infrastructure.BaseTestFixture;

namespace MercuryEngine.Data.TegraTextureLib.Tests;

public partial class BctexTests : BaseMedTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bctex", "textures"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Explicit, Parallelizable(ParallelScope.All)]
	public void TestLoadBctex(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BCTEX file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bctex = new Bctex();

		try
		{
			bctex.Read(fileStream);

			ConvertAndSaveTextures(bctex, inFile, relativeTo);
		}
		catch (Exception ex)
		{
			TestContext.Error.WriteLine("Error converting texture:");
			TestContext.Error.WriteLine(ex);
			throw;
		}
	}

	private static void ConvertAndSaveTextures(Bctex bctex, string sourceFilePath, string relativeTo)
	{
		var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileDir = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", "BCTEX", relativePath);

		Directory.CreateDirectory(outFileDir);

		foreach (var (i, texture) in bctex.Textures.Pairs())
		{
			using var bitmap = texture.ToBitmap();

			var outFileNameSuffix = bctex.Textures.Count > 1 ? $".{i}.png" : ".png";
			var outFileName = ( bctex.TextureName ?? sourceFileName ) + outFileNameSuffix;
			var outFilePath = Path.Join(outFileDir, outFileName);
			using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

			bitmap.Encode(outFileStream, SKEncodedImageFormat.Png, 100);
		}
	}

	private static SKColorType GetColorType(XtxImageFormat format)
		=> format switch {
			XtxImageFormat.NvnFormatRGBA8 => SKColorType.Rgba8888,
			// XtxImageFormat.DXT5           => SKColorType.Alpha8,
			_ => SKColorType.Unknown,
		};
}