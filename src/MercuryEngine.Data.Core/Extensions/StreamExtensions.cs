using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Extensions;

public static class StreamExtensions
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

	/// <summary>
	/// Reads exactly <paramref name="count"/> bytes from the provided <paramref name="stream"/>,
	/// and restores the <see cref="Stream.Position"/> property to the position before the read
	/// occurred..
	/// </summary>
	public static byte[] Peek(this Stream stream, long count)
	{
		if (!stream.CanSeek)
			throw new InvalidOperationException($"Stream must support seeking to use {nameof(Peek)}");

		var originalPosition = stream.Position;

		try
		{
			return stream.Read(count);
		}
		finally
		{
			stream.Position = originalPosition;
		}
	}

	public static long GetRealPosition(this Stream stream)
		=> stream switch {
			SlicedStream { HideRealPosition: false } slicedStream
				=> slicedStream.BaseStream.GetRealPosition(),

			_ => stream.Position,
		};

	/// <summary>
	/// Returns the number of bytes by which this <see cref="Stream"/> must be advanced in order
	/// to align its <see cref="Stream.Position"/> with an even block of <paramref name="byteAlignment"/> bytes.
	/// </summary>
	public static uint GetNeededPaddingForAlignment(this Stream stream, uint byteAlignment)
		=> MathHelper.GetNeededPaddingForAlignment((ulong) stream.Position, byteAlignment);

	/// <summary>
	/// Seeks this <see cref="Stream"/> to the new location given by <paramref name="newOffset"/> and <paramref name="origin"/>,
	/// and returns an <see cref="IDisposable"/> <see cref="SeekToken"/> that, when disposed, will return the stream to the
	/// position it was at when this method was first called.
	/// </summary>
	public static SeekToken TemporarySeek(this Stream stream, long newOffset, SeekOrigin origin = SeekOrigin.Begin)
	{
		var originalPosition = stream.Position;
		stream.Seek(newOffset, origin);
		return new SeekToken(stream, originalPosition);
	}

	public readonly struct SeekToken(Stream stream, long originalPosition) : IDisposable
	{
		public void Dispose()
			=> stream.Position = originalPosition;
	}
}