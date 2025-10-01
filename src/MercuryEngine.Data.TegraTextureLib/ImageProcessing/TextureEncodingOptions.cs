using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.TegraTextureLib.ImageProcessing;

public class TextureEncodingOptions
{
	public CompressionQuality CompressionQuality { get; set; } = CompressionQuality.Balanced;
	public int                MaxMipLevel        { get; set; } = XtxTextureInfo.MaxMipCount;
	public bool               Parallel           { get; set; } = true;
	public int                MaxParallelTasks   { get; set; } = Math.Max(4, Environment.ProcessorCount / 2);

	public IProgress<ProgressElement>? Progress { get; set; }
}