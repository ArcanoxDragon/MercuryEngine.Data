using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TegraSwizzle;
using NativeBlockDim = TegraSwizzle.BlockDim;

namespace MercuryEngine.Data.TegraTextureLib;

[PublicAPI]
public class TegraSwizzle
{
	public static uint GetBlockHeight(uint heightInBytes)
		=> NativeMethods.block_height_mip0(heightInBytes);

	public static uint GetMipBlockHeight(uint mipHeightInBytes, uint blockHeightMip0)
		=> NativeMethods.mip_block_height(mipHeightInBytes, blockHeightMip0);

	public static ulong GetSwizzledSurfaceSize(uint width, uint height, uint depth, BlockDim blockDim, uint blockHeightMip0, uint bytesPerPixel, uint mipmapCount = 1, uint arrayCount = 1)
	{
		NativeBlockDim nativeBlockDim = Unsafe.As<BlockDim, NativeBlockDim>(ref blockDim);
		return NativeMethods.swizzled_surface_size(width, height, depth, nativeBlockDim, blockHeightMip0, bytesPerPixel, mipmapCount, arrayCount);
	}

	public static unsafe void DeswizzleBlockLinear(uint width, uint height, uint depth, ReadOnlySpan<byte> source, Span<byte> destination, uint blockHeight, uint bytesPerPixel)
	{
		var expectedSize = width * height * depth * bytesPerPixel;

		if (destination.Length < expectedSize)
			throw new ArgumentException($"Destination must be at least {expectedSize} bytes, but was only {destination.Length}");

		fixed (byte* sourcePtr = source)
		fixed (byte* destinationPtr = destination)
			NativeMethods.deswizzle_block_linear(width, height, depth, sourcePtr, (nuint) source.Length, destinationPtr, (nuint) destination.Length, blockHeight, bytesPerPixel);
	}
}