using System.IO.Compression;

namespace MercuryEngine.Data.Utility;

public static class GzipHelper
{
	private const ulong GzipMagic = 0x088B1F;

	public static bool IsDataCompressed(ReadOnlySpan<byte> data)
	{
		if (data.Length < 8)
			return false;

		var magic = BitConverter.ToUInt64(data[..8]);

		return magic == GzipMagic;
	}

	public static byte[] DecompressData(byte[] data)
	{
		using var sourceStream = new MemoryStream(data);
		using var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress);
		using var destinationStream = new MemoryStream();

		gzipStream.CopyTo(destinationStream);

		return destinationStream.ToArray();
	}

	public static byte[] CompressData(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
	{
		using var sourceStream = new MemoryStream(data);
		using var destinationStream = new MemoryStream();
		using var gzipStream = new GZipStream(destinationStream, compressionLevel);

		sourceStream.CopyTo(gzipStream);
		gzipStream.Flush();

		return destinationStream.ToArray();
	}
}