using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.Utility;

namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

internal static class SwizzleUtility
{
	public static byte[] DeswizzleTextureData(TegraTexture texture, int arrayLevel, int mipLevel)
		=> DeswizzleTextureData(texture, arrayLevel, mipLevel, out _);

	public static byte[] DeswizzleTextureData(TegraTexture texture, int arrayLevel, int mipLevel, out FormatInfo formatInfo)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayLevel, (int) texture.Info.ArrayCount);

		var textureFormat = texture.Info.ImageFormat.ToTextureFormat();

		if (!FormatTable.TryGetTextureFormatInfo(textureFormat, out formatInfo))
			throw new NotSupportedException($"Unsupported texture format: {textureFormat}");

		var blockHeightMip0 = TegraSwizzle.GetBlockHeight(BitUtils.DivRoundUp(texture.Info.Height, formatInfo.BlockHeight));
		var arrayOffset = texture.Info.SliceSize * arrayLevel;
		var mipOffset = 0L;

		for (var m = 0; m < texture.Info.MipCount; m++)
		{
			const uint MipDepth = 1;

			var mipWidth = Math.Max(1, texture.Info.Width >> m);
			var mipHeight = Math.Max(1, texture.Info.Height >> m);
			var mipSize = GetMipSize(texture.Info, m);

			if (m == mipLevel)
			{
				var mipHeightInBlocks = BitUtils.DivRoundUp(mipHeight, formatInfo.BlockHeight);
				var mipBlockHeight = TegraSwizzle.GetMipBlockHeight(mipHeightInBlocks, blockHeightMip0);
				var blockHeightLog2 = (int) Math.Floor(Math.Log2(mipBlockHeight));

				var mipStart = (int) ( arrayOffset + mipOffset );
				var mipEnd = mipStart + (int) mipSize;
				var mipData = texture.Data.AsSpan()[mipStart..mipEnd];

				var heightMip0 = 1 << Math.Max(0, Math.Min(5, blockHeightLog2));
				var widthBlocks = BitUtils.DivRoundUp(mipWidth, formatInfo.BlockWidth);
				var heightBlocks = BitUtils.DivRoundUp(mipHeight, formatInfo.BlockHeight);

				var destinationSize = widthBlocks * heightBlocks * MipDepth * formatInfo.BytesPerPixel;
				var destination = new byte[destinationSize];

				TegraSwizzle.DeswizzleBlockLinear(widthBlocks, heightBlocks, 1, mipData, destination, (uint) heightMip0, formatInfo.BytesPerPixel);

				return destination;
			}

			mipOffset += (long) mipSize;
		}

		return [];
	}

	public static uint SwizzleTextureData(XtxTextureInfo textureInfo, int mipLevel, ReadOnlySpan<byte> mipData, Span<byte> overallDestination)
	{
		var textureFormat = textureInfo.ImageFormat.ToTextureFormat();

		if (!FormatTable.TryGetTextureFormatInfo(textureFormat, out var formatInfo))
			throw new NotSupportedException($"Unsupported texture format: {textureFormat}");

		var blockHeightMip0 = TegraSwizzle.GetBlockHeight(BitUtils.DivRoundUp(textureInfo.Height, formatInfo.BlockHeight));
		var mipOffset = 0L;

		for (var m = 0; m < textureInfo.MipCount; m++)
		{
			var mipWidth = Math.Max(1, textureInfo.Width >> m);
			var mipHeight = Math.Max(1, textureInfo.Height >> m);
			var mipSize = GetMipSize(textureInfo, m);

			if (m == mipLevel)
			{
				var mipHeightInBlocks = BitUtils.DivRoundUp(mipHeight, formatInfo.BlockHeight);
				var mipBlockHeight = TegraSwizzle.GetMipBlockHeight(mipHeightInBlocks, blockHeightMip0);
				var blockHeightLog2 = (int) Math.Floor(Math.Log2(mipBlockHeight));

				var mipStart = (int) mipOffset;
				var mipEnd = mipStart + (int) mipSize;
				var mipDestination = overallDestination[mipStart..mipEnd];

				var heightMip0 = 1 << Math.Max(0, Math.Min(5, blockHeightLog2));
				var widthBlocks = BitUtils.DivRoundUp(mipWidth, formatInfo.BlockWidth);
				var heightBlocks = BitUtils.DivRoundUp(mipHeight, formatInfo.BlockHeight);

				TegraSwizzle.SwizzleBlockLinear(widthBlocks, heightBlocks, 1, mipData, mipDestination, (uint) heightMip0, formatInfo.BytesPerPixel);

				return (uint) mipOffset;
			}

			mipOffset += (long) mipSize;
		}

		throw new ArgumentException("Invalid mip level", nameof(mipLevel));
	}

	public static ulong GetMipSize(XtxTextureInfo textureInfo, int mipLevel)
	{
		var textureFormat = textureInfo.ImageFormat.ToTextureFormat();

		if (!FormatTable.TryGetTextureFormatInfo(textureFormat, out var formatInfo))
			throw new NotSupportedException($"Unsupported texture format: {textureFormat}");

		var blockDim = new BlockDim {
			width = formatInfo.BlockWidth,
			height = formatInfo.BlockHeight,
			depth = 1,
		};
		var blockHeightMip0 = TegraSwizzle.GetBlockHeight(BitUtils.DivRoundUp(textureInfo.Height, formatInfo.BlockHeight));
		var mipWidth = Math.Max(1, textureInfo.Width >> mipLevel);
		var mipHeight = Math.Max(1, textureInfo.Height >> mipLevel);

		const uint MipDepth = 1;

		return TegraSwizzle.GetSwizzledSurfaceSize(mipWidth, mipHeight, MipDepth, blockDim, blockHeightMip0, formatInfo.BytesPerPixel);
	}
}