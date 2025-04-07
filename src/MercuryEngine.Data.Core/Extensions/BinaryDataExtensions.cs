using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Extensions;

public static class BinaryDataExtensions
{
	private const int BufferSize       = 2048;
	private const int DefaultMaxLength = 1024 * 8; // 8 kB maximum by default

	private static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

	#region Synchronous

	public static string ReadTerminatedCString(this BinaryReader reader, int maxLength = DefaultMaxLength)
		=> reader.ReadTerminatedCString(DefaultEncoding, maxLength);

	public static string ReadTerminatedCString(this BinaryReader reader, Encoding encoding, int maxLength = DefaultMaxLength)
	{
		var buffer = new byte[BufferSize];
		var builder = new StringBuilder();
		var totalBytesRead = 0;

		while (reader.BaseStream.Read(buffer, 0, buffer.Length) is > 0 and var bytesRead)
		{
			var terminatorIndex = Array.IndexOf(buffer, (byte) '\0');

			if (terminatorIndex >= 0)
			{
				// Found a terminator!

				if (terminatorIndex + totalBytesRead > maxLength)
					// Too many bytes *would* be read if we tried to reach the terminator; we still need to bail
					break;

				builder.Append(encoding.GetString(buffer[..terminatorIndex]));

				// Seek the stream back to the byte after the terminator
				reader.BaseStream.Seek(-( bytesRead - terminatorIndex - 1 ), SeekOrigin.Current);

				return builder.ToString();
			}

			if (bytesRead + totalBytesRead > maxLength)
				// Too many bytes would be read if we continued; bail now
				break;

			// Flush our current buffer to the builder and keep reading
			builder.Append(encoding.GetString(buffer));
			totalBytesRead += bytesRead;
		}

		throw new InvalidDataException("Encountered end-of-stream or too many bytes while reading a terminated string");
	}

	public static void WriteTerminatedCString(this BinaryWriter writer, string text)
		=> writer.WriteTerminatedCString(DefaultEncoding, text);

	public static void WriteTerminatedCString(this BinaryWriter writer, Encoding encoding, string text)
	{
		using var textWriter = new StreamWriter(writer.BaseStream, encoding, leaveOpen: true);

		textWriter.Write(text);
		textWriter.Flush();
		writer.Write((byte) '\0'); // Write the terminator
	}

	#endregion

	#region Asynchronous

	public static Task<string> ReadTerminatedCStringAsync(this AsyncBinaryReader reader, int maxLength = DefaultMaxLength, CancellationToken cancellationToken = default)
		=> reader.ReadTerminatedCStringAsync(DefaultEncoding, maxLength, cancellationToken);

	public static async Task<string> ReadTerminatedCStringAsync(this AsyncBinaryReader reader, Encoding encoding, int maxLength = DefaultMaxLength, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[BufferSize];
		var builder = new StringBuilder();
		var totalBytesRead = 0;

		Task<int> ReadChunkAsync()
			=> reader.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

		while (await ReadChunkAsync().ConfigureAwait(false) is > 0 and var bytesRead)
		{
			var terminatorIndex = Array.IndexOf(buffer, (byte) '\0');

			if (terminatorIndex >= 0)
			{
				// Found a terminator!

				if (terminatorIndex + totalBytesRead > maxLength)
					// Too many bytes *would* be read if we tried to reach the terminator; we still need to bail
					break;

				builder.Append(encoding.GetString(buffer[..terminatorIndex]));

				// Seek the stream back to the byte after the terminator
				reader.BaseStream.Seek(-( bytesRead - terminatorIndex - 1 ), SeekOrigin.Current);

				return builder.ToString();
			}

			if (bytesRead + totalBytesRead > maxLength)
				// Too many bytes would be read if we continued; bail now
				break;

			// Flush our current buffer to the builder and keep reading
			builder.Append(encoding.GetString(buffer));
			totalBytesRead += bytesRead;
		}

		throw new InvalidDataException("Encountered end-of-stream or too many bytes while reading a terminated string");
	}

	public static Task WriteTerminatedCStringAsync(this AsyncBinaryWriter writer, string text, CancellationToken cancellationToken = default)
		=> writer.WriteTerminatedCStringAsync(DefaultEncoding, text, cancellationToken);

	public static async Task WriteTerminatedCStringAsync(this AsyncBinaryWriter writer, Encoding encoding, string text, CancellationToken cancellationToken = default)
	{
		var stream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
		await using var textWriter = new StreamWriter(stream, encoding, leaveOpen: true);

		await textWriter.WriteAsync(text).ConfigureAwait(false);
		await textWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync((byte) '\0', cancellationToken).ConfigureAwait(false); // Write the terminator
	}

	#endregion
}