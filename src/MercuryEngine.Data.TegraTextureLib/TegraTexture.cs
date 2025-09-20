using SkiaSharp;
using MercuryEngine.Data.TegraTextureLib.Utility;

namespace MercuryEngine.Data.TegraTextureLib;

public sealed record TegraTexture(Xtx.TextureInfo Info, byte[] Data)
{
	public SKBitmap ToBitmap()
	{
		var deswizzledData = SwizzleUtility.DeswizzleTextureData(this, 0, 0, out var textureFormatInfo);
		ReadOnlySpan<byte> bitmapData;
		MemoryOwner<byte>? memoryOwner = null;

		if (Info.ImageFormat == XtxImageFormat.DXT1)
		{
			// Decode BC1
			memoryOwner = BCnDecoder.DecodeBC1(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1);
			bitmapData = memoryOwner.Span;
		}
		else if (Info.ImageFormat == XtxImageFormat.DXT5)
		{
			// Decode BC3
			memoryOwner = BCnDecoder.DecodeBC3(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1);
			bitmapData = memoryOwner.Span;
		}
		else if (Info.ImageFormat is XtxImageFormat.BC4U or XtxImageFormat.BC4S)
		{
			var signed = Info.ImageFormat == XtxImageFormat.BC4S;

			// Decode BC4
			memoryOwner = BCnDecoder.DecodeBC4(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, signed);
			bitmapData = memoryOwner.Span;
		}
		else if (Info.ImageFormat is XtxImageFormat.BC5U or XtxImageFormat.BC5S)
		{
			var signed = Info.ImageFormat == XtxImageFormat.BC5S;

			// Decode BC5
			memoryOwner = BCnDecoder.DecodeBC5(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, signed);
			bitmapData = memoryOwner.Span;
		}
		else
		{
			// Raw data
			bitmapData = deswizzledData;
		}

		SKBitmap bitmap;

		using (memoryOwner)
		{
			// First, load the data into a source bitmap with a color type corresponding to the source data
			var sourceColorType = GetColorType(textureFormatInfo);
			using var sourceBitmap = new SKBitmap((int) Info.Width, (int) Info.Height, sourceColorType, SKAlphaType.Unpremul);

			unsafe
			{
				var bitmapPixels = sourceBitmap.GetPixels();
				var pixelsSpan = new Span<byte>(bitmapPixels.ToPointer(), sourceBitmap.ByteCount);

				bitmapData.CopyTo(pixelsSpan);
			}

			// Next, copy the data to a new bitmap using a standard "Rgb888x" or "Rgba8888" format that is more widely supported
			var targetColorType = sourceColorType == SKColorType.Rgba8888 ? SKColorType.Rgba8888 : SKColorType.Rgb888x;

			bitmap = sourceBitmap.Copy(targetColorType);
		}

		GC.Collect();
		return bitmap;
	}

	private static SKColorType GetColorType(FormatInfo formatInfo)
		=> formatInfo.Components switch {
			1 => SKColorType.Gray8,
			2 => SKColorType.Rg88,
			3 => SKColorType.Rgb888x,
			_ => SKColorType.Rgba8888,
		};
}