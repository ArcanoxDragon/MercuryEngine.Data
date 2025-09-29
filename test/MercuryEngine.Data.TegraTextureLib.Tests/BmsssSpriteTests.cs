using ImageMagick;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.TegraTextureLib.Extensions;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.TegraTextureLib.Tests;

[TestFixture]
public class BmsssSpriteTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bmsss"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public async Task DumpBmsssSprites(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BMSSS file: {0}", inFile);

		await using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bmsss = new Bmsss();

		await bmsss.ReadAsync(fileStream);
		await DumpBmsssSpritesAsync(bmsss, inFile, relativeTo);
	}

	private static async Task DumpBmsssSpritesAsync(Bmsss bmsss, string sourceFilePath, string relativeTo)
	{
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileBaseDir = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", bmsss.DisplayName, relativePath);

		foreach (var (spriteSheetName, spriteSheet) in bmsss.SpriteSheets)
		{
			if (spriteSheet.Items is not { } spriteItems)
				continue;

			var bctexPath = Path.Join(RomFsPath, "textures", spriteSheet.TexturePath);

			if (!File.Exists(bctexPath))
			{
				await TestContext.Out.WriteLineAsync($"Skipping sprite sheet because BCTEX file does not exist: {bctexPath}");
				continue;
			}

			var bctex = new Bctex();
			var spriteSheetDir = Path.Join(outFileBaseDir, spriteSheetName);

			Directory.CreateDirectory(spriteSheetDir);

			await using (var fileStream = File.Open(bctexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				await bctex.ReadAsync(fileStream).ConfigureAwait(false);

			var spriteSheetImage = bctex.Textures.First().ToImage();

			foreach (var (i, spriteItem) in spriteItems.Pairs())
			{
				if (spriteItem is not { Value: { } sprite })
					continue;

				var uvs = sprite.TexUVs ?? new GUI__CSpriteSheetItem__STexUV();
				var uvOffset = uvs.Offset ?? new Vector2();
				var uvSize = uvs.Scale ?? new Vector2();

				var sourceX = (int) ( uvOffset.X * spriteSheetImage.Width );
				var sourceY = (int) ( uvOffset.Y * spriteSheetImage.Height );
				var sourceWidth = (uint) ( Math.Min(1, uvSize.X) * spriteSheetImage.Width );
				var sourceHeight = (uint) ( Math.Min(1, uvSize.Y) * spriteSheetImage.Height );

				using var spriteImage = spriteSheetImage.CloneArea(sourceX, sourceY, sourceWidth, sourceHeight);
				var outFileName = Path.Join(spriteSheetDir, $"{sprite.ID ?? $"sprite_{i}"}.png");
				await using var fileStream = File.Open(outFileName, FileMode.Create, FileAccess.Write);

				await spriteImage.WriteAsync(fileStream, MagickFormat.Png);
			}
		}
	}
}