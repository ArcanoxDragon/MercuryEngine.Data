using ImageMagick;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.Tests;

namespace MercuryEngine.Data.TegraTextureLib.Tests;

public partial class BctexTests
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

	[TestCaseSource(nameof(GetTestFiles)), Explicit, Parallelizable(ParallelScope.All)]
	public async Task TestCompareBctexAsync(string inFile, string relativeTo)
	{
		const int HeaderSize = 0x10;

		TestContext.Progress.WriteLine("Comparing BCTEX file: {0}", inFile);

		using var originalStream = new MemoryStream();
		using var rewrittenStream = new MemoryStream();
		byte[] originalRawData;
		byte[] rewrittenRawData;

		await using (var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			await fileStream.CopyToAsync(originalStream);

		var bctex = new Bctex();

		try
		{
			originalStream.Position = 0;
			await bctex.ReadAsync(originalStream);
			originalRawData = bctex.RawData;
			originalStream.Position = 0;

			await bctex.WriteAsync(rewrittenStream);
			rewrittenRawData = bctex.RawData;
			rewrittenStream.Position = 0;
		}
		catch (Exception ex)
		{
			await TestContext.Error.WriteLineAsync("Error reading or writing texture:");
			TestContext.Error.WriteLine(ex);
			throw;
		}

		var originalData = originalStream.ToArray();
		var rewrittenData = rewrittenStream.ToArray();
		var originalSpan = (ReadOnlySpan<byte>) originalData;
		var rewrittenSpan = (ReadOnlySpan<byte>) rewrittenData;

		if (Global.WriteOutputFiles)
		{
			var sourceFileName = Path.GetFileNameWithoutExtension(inFile);
			var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, inFile))!;
			var outFileDir = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", "BCTEX", relativePath);

			Directory.CreateDirectory(outFileDir);

			var originalOutFilePath = Path.Join(outFileDir, $"{sourceFileName}.orig.bctex");
			var rewrittenOutFilePath = Path.Join(outFileDir, $"{sourceFileName}.new.bctex");

			await File.WriteAllBytesAsync(originalOutFilePath, originalData);
			await File.WriteAllBytesAsync(rewrittenOutFilePath, rewrittenData);
		}

		// Re-capture the spans (since we crossed an await boundary)
		originalSpan = (ReadOnlySpan<byte>) originalData;
		rewrittenSpan = (ReadOnlySpan<byte>) rewrittenData;

		// Compare only the headers first
		CompareBuffers(originalSpan[..HeaderSize], rewrittenSpan[..HeaderSize]);

		// Compare the RAW data (not compressed)
		CompareBuffers(originalRawData, rewrittenRawData);
	}

	private static async Task ConvertAndSaveTexturesAsync(Bctex bctex, string sourceFilePath, string relativeTo)
	{
		var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileDir = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", "BCTEX", relativePath);

		Directory.CreateDirectory(outFileDir);

		var texture = bctex.Textures.Single();

		for (var arrayLevel = 0; arrayLevel < texture.Info.ArrayCount; arrayLevel++)
		{
			using var image = texture.ToImage(arrayLevel, isSrgb: bctex.IsSrgb);
			// using var convertedBitmap = ConvertBitmapColors(bitmap);
			var isHdr = texture.Info.ImageFormat == XtxImageFormat.BC6U;
			string fileNameSuffix;

			if (isHdr)
				fileNameSuffix = texture.Info.ArrayCount == 1 ? ".hdr" : $".{arrayLevel}.hdr";
			else
				fileNameSuffix = texture.Info.ArrayCount == 1 ? ".dds" : $".{arrayLevel}.dds";

			var outFileName = ( bctex.TextureName ?? sourceFileName ) + fileNameSuffix;
			var outFilePath = Path.Join(outFileDir, outFileName);
			var retry = false;
			var iterations = 0;

			do
			{
				retry = false;

				try
				{
					await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

					await image.WriteAsync(outFileStream, isHdr ? MagickFormat.Hdr : MagickFormat.Dds);
				}
				catch (IOException)
				{
					retry = true;
					Thread.Sleep(1000);
				}
			}
			while (retry && ( ++iterations <= 10 ));
		}
	}
}