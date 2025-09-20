using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using MercuryEngine.Data.TegraTextureLib.Extensions;

namespace MercuryEngine.Data.TegraTextureLib.Tests;

[TestFixture]
public class BmsssSpriteTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in BaseTestFixture.GetTestCasesFromRomFs("bmsss"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(BmsssSpriteTests.GetTestFiles)), Parallelizable]
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

			var spriteSheetBitmap = bctex.Textures.First().ToBitmap();

			foreach (var (i, spriteItem) in spriteItems.Pairs())
			{
				if (spriteItem is not { Value: { } sprite })
					continue;

				var uvs = sprite.TexUVs ?? new GUI__CSpriteSheetItem__STexUV();
				var uvOffset = uvs.Offset ?? new Vector2();
				var uvSize = uvs.Scale ?? new Vector2();

				var sourceX = (int) ( uvOffset.X * spriteSheetBitmap.Width );
				var sourceY = (int) ( uvOffset.Y * spriteSheetBitmap.Height );
				var sourceWidth = (int) ( Math.Min(1, uvSize.X) * spriteSheetBitmap.Width );
				var sourceHeight = (int) ( Math.Min(1, uvSize.Y) * spriteSheetBitmap.Height );
				var sourceRect = new SKRectI(sourceX, sourceY, sourceX + sourceWidth, sourceY + sourceHeight);
				var destRect = new SKRectI(0, 0, sourceWidth, sourceHeight);

				var spriteBitmapInfo = spriteSheetBitmap.Info with {
					Width = sourceRect.Width,
					Height = sourceRect.Height,
				};
				using var spriteBitmap = new SKBitmap(spriteBitmapInfo);

				using (var canvas = new SKCanvas(spriteBitmap))
					canvas.DrawBitmap(spriteSheetBitmap, sourceRect, destRect);

				var outFileName = Path.Join(spriteSheetDir, $"{sprite.ID ?? $"sprite_{i}"}.png");
				await using var fileStream = File.Open(outFileName, FileMode.Create, FileAccess.Write);

				spriteBitmap.Encode(fileStream, SKEncodedImageFormat.Png, 100);
			}
		}
	}
}