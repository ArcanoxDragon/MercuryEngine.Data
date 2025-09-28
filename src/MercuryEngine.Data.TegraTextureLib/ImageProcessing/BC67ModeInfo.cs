namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

internal readonly struct BC7ModeInfo(
	int subsetCount,
	int partitionBitsCount,
	int pBits,
	int rotationBitCount,
	int indexModeBitCount,
	int colorIndexBitCount,
	int alphaIndexBitCount,
	int colorDepth,
	int alphaDepth)
{
	public readonly int SubsetCount        = subsetCount;
	public readonly int PartitionBitCount  = partitionBitsCount;
	public readonly int PBits              = pBits;
	public readonly int RotationBitCount   = rotationBitCount;
	public readonly int IndexModeBitCount  = indexModeBitCount;
	public readonly int ColorIndexBitCount = colorIndexBitCount;
	public readonly int AlphaIndexBitCount = alphaIndexBitCount;
	public readonly int ColorDepth         = colorDepth;
	public readonly int AlphaDepth         = alphaDepth;
}