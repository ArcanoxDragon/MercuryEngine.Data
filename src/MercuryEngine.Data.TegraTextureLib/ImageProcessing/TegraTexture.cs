using System.Buffers;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.Utility;
using SkiaSharp;

namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

public sealed record TegraTexture(Xtx.TextureInfo Info, byte[] Data)
{
	// Linear color space is used when decoding and converting textures so that we preserve the raw color ranges in the texture data.
	private static readonly SKColorSpace LinearColorSpace = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Xyz);

	public SKBitmap ToBitmap()
	{
		SKBitmap bitmap = null!;

		using (var memoryOwner = DeswizzleAndDecode(out var textureFormatInfo))
		{
			// First, load the data into a source bitmap with a color type corresponding to the source data
			var bitmapData = memoryOwner.Span;
			var sourceColorType = GetColorType(textureFormatInfo);
			SKBitmap? sourceBitmap = null;

			try
			{
				sourceBitmap = new SKBitmap((int) Info.Width, (int) Info.Height, sourceColorType, SKAlphaType.Unpremul, LinearColorSpace);

				unsafe
				{
					var bitmapPixels = sourceBitmap.GetPixels();
					var pixelsSpan = new Span<byte>(bitmapPixels.ToPointer(), sourceBitmap.ByteCount);

					bitmapData.CopyTo(pixelsSpan);
				}

				if (sourceColorType is SKColorType.Rgb888x or SKColorType.Rgba8888)
				{
					// Can use the source bitmap directly
					bitmap = sourceBitmap;
				}
				else
				{
					// If necessary, copy the data to a new bitmap using a standard "Rgb888x" format that is more widely supported than the ones with fewer than 3 components
					bitmap = new SKBitmap((int) Info.Width, (int) Info.Height, SKColorType.Rgb888x, SKAlphaType.Unpremul, LinearColorSpace);
					sourceBitmap.CopyTo(bitmap, bitmap.ColorType);
				}
			}
			finally
			{
				if (!ReferenceEquals(bitmap, sourceBitmap))
					sourceBitmap?.Dispose();
			}
		}

		GC.Collect();
		return bitmap;
	}

	public IMemoryOwner<byte> ToRawData()
		=> DeswizzleAndDecode(out _);

	private MemoryOwner<byte> DeswizzleAndDecode(out FormatInfo textureFormatInfo)
	{
		var deswizzledData = SwizzleUtility.DeswizzleTextureData(this, 0, 0, out textureFormatInfo);
		MemoryOwner<byte> memoryOwner;

		if (Info.ImageFormat == XtxImageFormat.DXT1)
		{
			// Decode BC1
			memoryOwner = BCnDecoder.DecodeBC1(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1);
		}
		else if (Info.ImageFormat == XtxImageFormat.DXT5)
		{
			// Decode BC3
			memoryOwner = BCnDecoder.DecodeBC3(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1);
		}
		else if (Info.ImageFormat is XtxImageFormat.BC4U or XtxImageFormat.BC4S)
		{
			var signed = Info.ImageFormat == XtxImageFormat.BC4S;

			// Decode BC4
			memoryOwner = BCnDecoder.DecodeBC4(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, signed);
		}
		else if (Info.ImageFormat is XtxImageFormat.BC5U or XtxImageFormat.BC5S)
		{
			var signed = Info.ImageFormat == XtxImageFormat.BC5S;

			// Decode BC5
			memoryOwner = BCnDecoder.DecodeBC5(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, signed);
		}
		else
		{
			// Raw data
			memoryOwner = MemoryOwner<byte>.RentCopy(deswizzledData);
		}

		return memoryOwner;
	}

	private static SKColorType GetColorType(FormatInfo formatInfo)
		=> formatInfo.Components switch {
			1 => SKColorType.Gray8,
			2 => SKColorType.Rg88,
			3 => SKColorType.Rgb888x,
			_ => SKColorType.Rgba8888,
		};
}