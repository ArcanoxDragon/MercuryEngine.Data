using System.Diagnostics;
using SkiaSharp;

namespace MercuryEngine.Data.Converters.Bcmdl;

internal static class TextureConverter
{
	public static (byte[], byte[]?) SeparateBaseColorAndEmissive(byte[] inputTexture)
	{
		SKImageInfo inputImageInfo;

		using (var inputStream = new MemoryStream(inputTexture))
		using (var inputCodec = SKCodec.Create(inputStream))
			inputImageInfo = inputCodec.Info;

		using var sourceBitmap = SKBitmap.Decode(inputTexture, inputImageInfo with {
			AlphaType = SKAlphaType.Unpremul,
		});

		if (sourceBitmap.ColorType is not (SKColorType.Rgba8888 or SKColorType.Bgra8888))
			return ( inputTexture, null );

		using var baseColorTexture = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
		using var emissiveTexture = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);

		// Multiply the alpha component of the source texture with the RGB components to premultiply the emissive map
		{
			using var sourcePixels = sourceBitmap.PeekPixels();
			using var baseColorPixels = baseColorTexture.PeekPixels();
			using var emissivePixels = emissiveTexture.PeekPixels();

			var sourceData = sourcePixels.GetPixelSpan<Rgba>();
			var baseColorData = baseColorPixels.GetPixelSpan<Rgba>();
			var emissiveData = emissivePixels.GetPixelSpan<Rgba>();

			Debug.Assert(sourceData.Length == baseColorData.Length);
			Debug.Assert(sourceData.Length == emissiveData.Length);

			for (var i = 0; i < sourceData.Length; i++)
			{
				var sourcePixel = sourceData[i];

				if (sourceBitmap.ColorType == SKColorType.Bgra8888)
				{
					// Need to swap R and B
					sourcePixel = sourcePixel with {
						R = sourcePixel.B,
						B = sourcePixel.R,
					};
				}

				var sourceR = sourcePixel.R / 255f;
				var sourceG = sourcePixel.G / 255f;
				var sourceB = sourcePixel.B / 255f;
				var sourceA = sourcePixel.A / 255f;

				baseColorData[i] = sourcePixel with { A = 255 };

				var emissiveR = sourceR * sourceA;
				var emissiveG = sourceG * sourceA;
				var emissiveB = sourceB * sourceA;

				emissiveData[i] = new Rgba((byte) ( emissiveR * 255 ), (byte) ( emissiveG * 255 ), (byte) ( emissiveB * 255 ), 255);
			}
		}

		using var baseColorStream = new MemoryStream();
		using var emissiveStream = new MemoryStream();

		baseColorTexture.Encode(baseColorStream, SKEncodedImageFormat.Png, 100);
		emissiveTexture.Encode(emissiveStream, SKEncodedImageFormat.Png, 100);

		return ( baseColorStream.ToArray(), emissiveStream.ToArray() );
	}

	private readonly record struct Rgba(byte R, byte G, byte B, byte A);
}