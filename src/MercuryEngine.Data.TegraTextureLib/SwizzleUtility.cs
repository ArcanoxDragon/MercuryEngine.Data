using MercuryEngine.Data.TegraTextureLib.Utility;

namespace MercuryEngine.Data.TegraTextureLib;

internal static class SwizzleUtility
{
	public static byte[] DeswizzleTextureData(TegraTexture texture, int arrayLevel, int mipLevel)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayLevel, (int) texture.Info.ArrayCount);

		var textureFormat = texture.Info.ImageFormat.ToTextureFormat();

		if (!FormatTable.TryGetTextureFormatInfo(textureFormat, out var formatInfo))
			throw new NotSupportedException($"Unsupported texture format: {textureFormat}");

		var blockDim = new BlockDim {
			width = formatInfo.BlockWidth,
			height = formatInfo.BlockHeight,
			depth = 1,
		};
		var blockHeightMip0 = TegraSwizzle.GetBlockHeight(BitUtils.DivRoundUp(texture.Info.Height, formatInfo.BlockHeight));

		var arrayOffset = texture.Info.SliceSize * arrayLevel;
		var mipOffset = 0L;

		for (var m = 0; m < texture.Info.MipCount; m++)
		{
			const uint MipDepth = 1;

			var mipWidth = Math.Max(1, texture.Info.Width >> m);
			var mipHeight = Math.Max(1, texture.Info.Height >> m);
			var mipSize = TegraSwizzle.GetSwizzledSurfaceSize(mipWidth, mipHeight, MipDepth, blockDim, blockHeightMip0, formatInfo.BytesPerPixel);

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
}