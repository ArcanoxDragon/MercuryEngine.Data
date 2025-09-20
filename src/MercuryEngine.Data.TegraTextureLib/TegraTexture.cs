using SkiaSharp;
using MercuryEngine.Data.TegraTextureLib.Utility;

namespace MercuryEngine.Data.TegraTextureLib;

public sealed record TegraTexture(Xtx.TextureInfo Info, byte[] Data)
{
	public SKBitmap ToBitmap()
	{
		var deswizzledData = SwizzleUtility.DeswizzleTextureData(this, 0, 0);
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
		else
		{
			// Raw data
			bitmapData = deswizzledData;
		}

		SKBitmap bitmap;

		using (memoryOwner)
		{
			bitmap = new SKBitmap((int) Info.Width, (int) Info.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

			unsafe
			{
				var bitmapPixels = bitmap.GetPixels();
				var pixelsSpan = new Span<byte>(bitmapPixels.ToPointer(), bitmap.ByteCount);

				bitmapData.CopyTo(pixelsSpan);
			}
		}

		GC.Collect();
		return bitmap;
	}
}