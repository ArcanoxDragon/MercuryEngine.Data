using BCnEncoder.Shared;
using MercuryEngine.Data.TegraTextureLib.ImageProcessing;

namespace MercuryEngine.Data.TegraTextureLib.Extensions;

public static class XtxFormatExtensions
{
	public static XtxImageFormat ToXtxImageFormat(this CompressionFormat compressionFormat)
		=> compressionFormat switch {
			CompressionFormat.R            => XtxImageFormat.NvnFormatR8,
			CompressionFormat.Rg           => XtxImageFormat.NvnFormatRG8,
			CompressionFormat.Rgba         => XtxImageFormat.NvnFormatRGBA8,
			CompressionFormat.Bc1          => XtxImageFormat.DXT1,
			CompressionFormat.Bc1WithAlpha => XtxImageFormat.DXT1,
			CompressionFormat.Bc3          => XtxImageFormat.DXT5,
			CompressionFormat.Bc4          => XtxImageFormat.BC4U,
			CompressionFormat.Bc5          => XtxImageFormat.BC5U,

			_ => default,
		};

	public static CompressionFormat ToCompressionFormat(this XtxImageFormat imageFormat)
		=> imageFormat switch {
			XtxImageFormat.NvnFormatR8    => CompressionFormat.R,
			XtxImageFormat.NvnFormatRG8   => CompressionFormat.Rg,
			XtxImageFormat.NvnFormatRGBA8 => CompressionFormat.Rgba,
			XtxImageFormat.DXT1           => CompressionFormat.Bc1,
			XtxImageFormat.DXT5           => CompressionFormat.Bc3,
			XtxImageFormat.BC4U           => CompressionFormat.Bc4,
			XtxImageFormat.BC5U           => CompressionFormat.Bc5,

			_ => CompressionFormat.Unknown,
		};
}