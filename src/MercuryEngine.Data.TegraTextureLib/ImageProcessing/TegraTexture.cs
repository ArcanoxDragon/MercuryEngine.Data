using System.Buffers;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.Utility;
using SkiaSharp;

namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

public sealed record TegraTexture(XtxTextureInfo Info, byte[] Data)
{
	private static readonly SKColorSpace LinearColorSpace = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Identity);
	private static readonly SKColorSpace SrgbColorSpace   = SKColorSpace.CreateSrgb();

	public SKBitmap ToBitmap(bool isSrgb = true)
	{
		SKBitmap bitmap = null!;

		using (var memoryOwner = DeswizzleAndDecode(out var textureFormatInfo, out var wasDecoded))
		{
			// First, load the data into a source bitmap with a color type corresponding to the source data
			var bitmapData = memoryOwner.Span;
			var sourceColorType = GetColorType(textureFormatInfo, wasDecoded);

			bitmap = new SKBitmap((int) Info.Width, (int) Info.Height, sourceColorType, SKAlphaType.Unpremul, isSrgb ? SrgbColorSpace : LinearColorSpace);

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

	public IMemoryOwner<byte> ToRawData()
		=> DeswizzleAndDecode(out _, out _);

	private MemoryOwner<byte> DeswizzleAndDecode(out FormatInfo textureFormatInfo, out bool wasDecoded)
	{
		var deswizzledData = SwizzleUtility.DeswizzleTextureData(this, 0, 0, out textureFormatInfo);
		MemoryOwner<byte> memoryOwner;

		if (Info.ImageFormat == XtxImageFormat.DXT1)
		{
			// Decode BC1
			memoryOwner = BCnDecoder.DecodeBC1(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1);
			wasDecoded = true;
		}
		else if (Info.ImageFormat == XtxImageFormat.DXT5)
		{
			// Decode BC3
			memoryOwner = BCnDecoder.DecodeBC3(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1);
			wasDecoded = true;
		}
		else if (Info.ImageFormat is XtxImageFormat.BC4U or XtxImageFormat.BC4S)
		{
			var signed = Info.ImageFormat == XtxImageFormat.BC4S;

			// Decode BC4
			memoryOwner = BCnDecoder.DecodeBC4(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, signed);
			wasDecoded = true;
		}
		else if (Info.ImageFormat is XtxImageFormat.BC5U or XtxImageFormat.BC5S)
		{
			var signed = Info.ImageFormat == XtxImageFormat.BC5S;

			// Decode BC5
			memoryOwner = BCnDecoder.DecodeBC5(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, signed);
			wasDecoded = true;
		}
		else
		{
			// Raw data
			memoryOwner = MemoryOwner<byte>.RentCopy(deswizzledData);
			wasDecoded = false;
		}

		return memoryOwner;
	}

	private static SKColorType GetColorType(FormatInfo formatInfo, bool wasDecoded)
	{
		if (wasDecoded)
		{
			// Always 4 bytes per pixel after decoding

			return formatInfo.Components switch {
				1 => SKColorType.R8Unorm,
				2 => SKColorType.Rg88,
				3 => SKColorType.Rgb888x,
				_ => SKColorType.Rgba8888,
			};
		}

		return ( formatInfo.Components, formatInfo.BytesPerPixel ) switch {
			// 1 Component
			(1, 16) => SKColorType.Alpha16,
			(1, _)  => SKColorType.R8Unorm,

			// 2 Components
			(2, 4) => SKColorType.Rg1616,
			(2, _) => SKColorType.Rg88,

			// 3 Components
			(3, 4) => SKColorType.Rgb101010x,
			(3, _) => SKColorType.Rgb565,

			// 4 Components
			(4, 16) => SKColorType.RgbaF32,
			(4, 8)  => SKColorType.Rgba16161616, // Or is it F16?
			(_, _)  => SKColorType.Rgba8888,
		};
	}
}