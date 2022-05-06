namespace MercuryEngine.Data.Extensions;

internal static class StreamExtensions
{
	/// <summary>
	/// Returns whether or not the provided <paramref name="stream"/> has at least
	/// <paramref name="bytes"/> bytes remaining between its current position and
	/// the end of the stream.
	/// </summary>
	public static bool HasBytes(this Stream stream, long bytes)
		=> stream.Length - stream.Position >= bytes;

	/// <summary>
	/// Reads exactly <paramref name="count"/> bytes from the provided <paramref name="stream"/>.
	/// </summary>
	public static byte[] Read(this Stream stream, long count)
	{
		var buffer = new byte[count];
		var read = stream.Read(buffer, 0, buffer.Length);

		if (read < count)
			throw new IOException($"Expected to read {count} bytes, but only got {read}");

		return buffer;
	}
}