using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace MercuryEngine.Data.Utility;

public static class GzipHelper
{
	private const ulong GzipMagic = 0x088B1F;

	private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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

	public static byte[] CompressData(byte[] data, CompressionLevel compressionLevel = CompressionLevel.BestCompression)
	{
		using var sourceStream = new MemoryStream(data);
		using var destinationStream = new MemoryStream();

		using (var gzipStream = new GZipStream(destinationStream, CompressionMode.Compress, compressionLevel))
		{
			gzipStream.LastModified = UnixEpoch; // Dread does not use the timestamp portion of the header, so it should be all 0s
			sourceStream.CopyTo(gzipStream);
			gzipStream.Flush();
		}

		// To get a 100% data match, we need to update a couple fields in the header
		var buffer = destinationStream.ToArray();

		buffer[0x8] = 0x2; // XFL = Best Compression
		buffer[0x9] = 0xa; // OS = 10 (Value used in base game)

		return buffer;
	}
}