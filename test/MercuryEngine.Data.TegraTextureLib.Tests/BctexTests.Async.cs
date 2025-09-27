using SkiaSharp;
using MercuryEngine.Data.TegraTextureLib.Extensions;
using MercuryEngine.Data.TegraTextureLib.Formats;
using BaseMedTestFixture = MercuryEngine.Data.Tests.Infrastructure.BaseTestFixture;

namespace MercuryEngine.Data.TegraTextureLib.Tests;

public partial class BctexTests : BaseMedTestFixture
{
	[TestCaseSource(nameof(GetTestFiles)), Explicit, Parallelizable(ParallelScope.All)]
	public async Task TestLoadBctexAsync(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BCTEX file: {0}", inFile);

		await using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bctex = new Bctex();

		try
		{
			await bctex.ReadAsync(fileStream);

			await BctexTests.ConvertAndSaveTexturesAsync(bctex, inFile, relativeTo);
		}
		catch (Exception ex)
		{
			await TestContext.Error.WriteLineAsync("Error converting texture:");
			TestContext.Error.WriteLine(ex);
			throw;
		}
	}

	private static async Task ConvertAndSaveTexturesAsync(Bctex bctex, string sourceFilePath, string relativeTo)
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
			await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

			bitmap.Encode(outFileStream, SKEncodedImageFormat.Png, 100);
		}
	}
}