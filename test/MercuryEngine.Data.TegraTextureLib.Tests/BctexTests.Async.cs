using MercuryEngine.Data.TegraTextureLib.Extensions;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.Tests;
using SkiaSharp;

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