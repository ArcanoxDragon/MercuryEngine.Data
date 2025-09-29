using System.Text;
using ImageMagick;
using ImageMagick.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.ImageProcessing;
using MercuryEngine.Data.Tests;
using BaseMedTestFixture = MercuryEngine.Data.Tests.Infrastructure.BaseTestFixture;

namespace MercuryEngine.Data.TegraTextureLib.Tests;

public partial class BctexTests : BaseMedTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bctex", "textures"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	private readonly List<(string, Bctex)> debugValues = [];

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		this.debugValues.Clear();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		var csvBuilder = new StringBuilder();

		csvBuilder.AppendLine("Texture Name, Unknown1, Unknown2, Image Format, Mip Count, Full Texture Path");

		foreach (var (texturePath, bctex) in this.debugValues)
		{
			var textureName = Path.GetFileName(texturePath);
			var unknown1 = bctex.Unknown2;
			var unknown2 = bctex.Unknown1;
			var texture = bctex.Textures[0];

			csvBuilder.AppendLine($"{textureName}, 0x{unknown1:X4}, 0x{unknown2:X2}, {texture.Info.ImageFormat}, {texture.Info.MipCount}, {texturePath}");
		}

		var csvPath = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", "BCTEX", "unknown1.csv");

		File.WriteAllText(csvPath, csvBuilder.ToString());
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

			var relativePath = Path.GetRelativePath(relativeTo, inFile);

			this.debugValues.Add(( relativePath, bctex ));

			if (Global.WriteOutputFiles)
				ConvertAndSaveTextures(bctex, inFile, relativeTo);
		}
		catch (Exception ex)
		{
			TestContext.Error.WriteLine("Error converting texture:");
			TestContext.Error.WriteLine(ex);
			throw;
		}

		var flag = false;

		var textureCount = bctex.Textures.Count;

		Assert.That(textureCount, Is.EqualTo(1));

		var texture = bctex.Textures[0];
		var textureFormat = texture.Info.ImageFormat.ToString();

		if (texture.Info.ImageFormat == XtxImageFormat.DXT5 && bctex.Unknown2 != 0x2)
			flag = true;
		if (texture.Info.ImageFormat == XtxImageFormat.BC5U && bctex.Unknown2 != 0xC)
			flag = true;
		if (texture.Info.ImageFormat == XtxImageFormat.NvnFormatR8 && bctex.Unknown2 != 0x4)
			flag = true;

		var textureFormat2 = texture.Info.ImageFormat.ToTextureFormat();

		Assert.That(FormatTable.TryGetTextureFormatInfo(textureFormat2, out var formatInfo));

		var message = $"{textureCount,2} textures | {textureFormat} | {formatInfo.Format} @ {formatInfo.BytesPerPixel} | {texture.Info.MipCount} Mips | 0x{bctex.Unknown2:X4} | 0x{bctex.Unknown1:X2}";

		if (flag)
			Assert.Fail(message);
		else
			Assert.Pass(message);
	}

	[TestCaseSource(nameof(GetTestFiles)), Explicit, Parallelizable(ParallelScope.All)]
	public void TestCompareBctex(string inFile, string relativeTo)
	{
		const int HeaderSize = 0x10;

		TestContext.Progress.WriteLine("Comparing BCTEX file: {0}", inFile);

		using var originalStream = new MemoryStream();
		using var rewrittenStream = new MemoryStream();
		byte[] originalRawData;
		byte[] rewrittenRawData;

		using (var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			fileStream.CopyTo(originalStream);

		var bctex = new Bctex();

		try
		{
			originalStream.Position = 0;
			bctex.Read(originalStream);
			originalRawData = bctex.RawData;
			originalStream.Position = 0;

			bctex.Write(rewrittenStream);
			rewrittenRawData = bctex.RawData;
			rewrittenStream.Position = 0;
		}
		catch (Exception ex)
		{
			TestContext.Error.WriteLine("Error reading or writing texture:");
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

			File.WriteAllBytes(originalOutFilePath, originalData);
			File.WriteAllBytes(rewrittenOutFilePath, rewrittenData);
		}

		// Compare only the headers first
		CompareBuffers(originalSpan[..HeaderSize], rewrittenSpan[..HeaderSize]);

		// Compare the RAW data (not compressed)
		CompareBuffers(originalRawData, rewrittenRawData);
	}

	private static void ConvertAndSaveTextures(Bctex bctex, string sourceFilePath, string relativeTo)
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
					using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

					image.Write(outFileStream, isHdr ? MagickFormat.Hdr : MagickFormat.Dds);
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