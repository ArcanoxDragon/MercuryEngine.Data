using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using ImageMagick;
using MercuryEngine.Data.TegraTextureLib.Extensions;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.Utility;
using Half = System.Half;

namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

public sealed record TegraTexture(XtxTextureInfo Info, byte[] Data)
{
	#region Static

	public static TegraTexture FromImage(MagickImage image, PixelMapping channelMapping, CompressionFormat compressionFormat, CompressionQuality compressionQuality = CompressionQuality.Balanced)
		=> FromImage(image, channelMapping.ToString(), compressionFormat, compressionQuality);

	public static TegraTexture FromImage(MagickImage image, string channelMapping, CompressionFormat compressionFormat, CompressionQuality compressionQuality = CompressionQuality.Balanced)
	{
		// Do this first so we can throw early for bad inputs
		var xtxImageFormat = compressionFormat.ToXtxImageFormat();

		if (xtxImageFormat == default)
			throw new ArgumentException($"Unsupported compression format \"{compressionFormat}\"");

		using var imagePixels = image.GetPixels();
		var imagePixelsData = imagePixels.ToByteArray(channelMapping);
		var encoder = new BcEncoder(compressionFormat) {
			OutputOptions = {
				FileFormat = OutputFileFormat.Dds,
				Quality = compressionQuality,
				MaxMipMapLevel = XtxTextureInfo.MaxMipCount,
			},
		};
		var ddsFile = encoder.EncodeToDds(imagePixelsData, (int) image.Width, (int) image.Height, channelMapping.Length >= 4 ? PixelFormat.Rgba32 : PixelFormat.Rgb24);

		if (ddsFile.Faces.Count != 1)
			// No support for encoding cubemaps currently
			throw new ApplicationException("Expected exactly one DDS face");

		var mainFace = ddsFile.Faces[0];
		var textureInfo = new XtxTextureInfo {
			Width = image.Width,
			Height = image.Height,
			MipCount = (uint) mainFace.MipMaps.Length,
			ImageFormat = xtxImageFormat,
		};
		var totalSwizzledDataSize = Enumerable.Range(0, mainFace.MipMaps.Length).Select(mipLevel => SwizzleUtility.GetMipSize(textureInfo, mipLevel)).Sum(s => (long) s);

		// Only one image in the texture, so the slice size is equal to the data size
		textureInfo.DataSize = (uint) totalSwizzledDataSize;
		textureInfo.SliceSize = (uint) totalSwizzledDataSize;

		var overallData = new byte[totalSwizzledDataSize];
		var overallDataSpan = overallData.AsSpan();

		for (var mipLevel = 0; mipLevel < mainFace.MipMaps.Length; mipLevel++)
		{
			var mipData = mainFace.MipMaps[mipLevel].Data.AsSpan();
			var mipOffset = SwizzleUtility.SwizzleTextureData(textureInfo, mipLevel, mipData, overallDataSpan);

			textureInfo.MipOffsets[mipLevel] = mipOffset;
		}

		return new TegraTexture(textureInfo, overallData);
	}

	#endregion

	public MagickImage ToImage(int arrayLevel = 0, int mipLevel = 0, bool isSrgb = true)
	{
		MagickImage image = null!;

		using (var memoryOwner = DeswizzleAndDecode(arrayLevel, mipLevel, out var textureFormatInfo))
		{
			// First, load the data into a source bitmap with a color type corresponding to the source data
			var bitmapData = memoryOwner.Span;
			var pixelMapping = GetPixelMapping(textureFormatInfo, out var storageType);
			var readSettings = new PixelReadSettings(Info.Width, Info.Height, storageType, pixelMapping) {
				ReadSettings = {
					ColorSpace = ColorSpace.Undefined,
				},
			};

			image = new MagickImage();

			if (Info.ImageFormat == XtxImageFormat.BC6U)
			{
				var floatPixels = ConvertHdrPixels(bitmapData);

				image.ReadPixels(floatPixels.Span, readSettings);
			}
			else
			{
				image.ReadPixels(bitmapData, readSettings);
			}

			image.ColorSpace = isSrgb ? ColorSpace.sRGB : ColorSpace.Undefined;
		}

		GC.Collect();
		return image;
	}

	public IMemoryOwner<byte> ToRawData(int arrayLevel = 0, int mipLevel = 0)
		=> DeswizzleAndDecode(arrayLevel, mipLevel, out _);

	private MemoryOwner<byte> DeswizzleAndDecode(int arrayLevel, int mipLevel, out FormatInfo textureFormatInfo)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(arrayLevel);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayLevel, (int) Info.ArrayCount);
		ArgumentOutOfRangeException.ThrowIfNegative(mipLevel);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mipLevel, (int) Info.MipCount);

		var deswizzledData = SwizzleUtility.DeswizzleTextureData(this, arrayLevel, mipLevel, out textureFormatInfo);
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
		else if (Info.ImageFormat == XtxImageFormat.BC6U)
		{
			// Decode BC6
			memoryOwner = BCnDecoder.DecodeBC6(deswizzledData, (int) Info.Width, (int) Info.Height, 1, 1, 1, false);
		}
		else
		{
			// Raw data
			memoryOwner = MemoryOwner<byte>.RentCopy(deswizzledData);
		}

		return memoryOwner;
	}

	private static MemoryOwner<float> ConvertHdrPixels(ReadOnlySpan<byte> rawData)
	{
		var halfPixels = MemoryMarshal.Cast<byte, Half>(rawData);
		var floatMemory = MemoryOwner<float>.Rent(halfPixels.Length);
		var floatSpan = floatMemory.Span;

		// Try accelerated conversions before falling back to per-pixel conversions
		if (halfPixels.Length % 8 == 0 && Vector256.IsHardwareAccelerated)
		{
			for (var i = 0; i < halfPixels.Length; i += 8)
			{
				floatSpan[i] = (float) halfPixels[i];
				floatSpan[i + 1] = (float) halfPixels[i + 1];
				floatSpan[i + 2] = (float) halfPixels[i + 2];
				floatSpan[i + 3] = (float) halfPixels[i + 3];
				floatSpan[i + 4] = (float) halfPixels[i + 4];
				floatSpan[i + 5] = (float) halfPixels[i + 5];
				floatSpan[i + 6] = (float) halfPixels[i + 6];
				floatSpan[i + 7] = (float) halfPixels[i + 7];

				var thisSpan = floatSpan[i..( i + 8 )];
				var values = Vector256.Create((ReadOnlySpan<float>) thisSpan);

				values *= 65535f;
				values.CopyTo(thisSpan);
			}
		}
		else if (halfPixels.Length % 4 == 0 && Vector128.IsHardwareAccelerated)
		{
			for (var i = 0; i < halfPixels.Length; i += 4)
			{
				floatSpan[i] = (float) halfPixels[i];
				floatSpan[i + 1] = (float) halfPixels[i + 1];
				floatSpan[i + 2] = (float) halfPixels[i + 2];
				floatSpan[i + 3] = (float) halfPixels[i + 3];

				var thisSpan = floatSpan[i..( i + 4 )];
				var values = Vector128.Create((ReadOnlySpan<float>) thisSpan);

				values *= 65535f;
				values.CopyTo(thisSpan);
			}
		}
		else
		{
			for (var i = 0; i < halfPixels.Length; i++)
				floatSpan[i] = (float) halfPixels[i] * 65535f;
		}

		return floatMemory;
	}

	private static string GetPixelMapping(FormatInfo formatInfo, out StorageType storageType)
	{
		if (formatInfo.Format is TextureImageFormat.Bc6HSfloat or TextureImageFormat.Bc6HUfloat)
		{
			// HDR is always RGBA, 16-bits per component
			storageType = StorageType.Quantum;
			return "RGBA";
		}

		storageType = StorageType.Char;

		return formatInfo.Components switch {
			1 => "R",
			2 => "RG",
			3 => "RGB",
			_ => "RGBA",
		};
	}
}