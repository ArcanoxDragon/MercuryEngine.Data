using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class TerminatedStringField(string initialValue) : BaseBinaryField<string>(initialValue)
{
	public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

	private const int BufferSize       = 2048;
	private const int DefaultMaxLength = 1024 * 8; // 8 kB maximum by default

	public TerminatedStringField() : this(string.Empty) { }

	/// <summary>
	/// Gets or sets the string value of this <see cref="TerminatedStringField"/>.
	/// </summary>
	public override string Value
	{
		get => base.Value;
		set
		{
			ArgumentNullException.ThrowIfNull(value);

			if (Encoding.GetByteCount(value) > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be larger than the maximum length of {MaxLength} bytes.");

			base.Value = value;

#if DEBUG
			InternalKnownStrings.Record(value);
#endif
		}
	}

	/// <summary>
	/// The maximum length of a string that can be read or written by this component.
	/// Strings larger than this will throw an exception. Defaults to 8kb (8192 ASCII characters).
	/// </summary>
	public int MaxLength { get; set; } = DefaultMaxLength;

	/// <summary>
	/// The encoding used when reading and writing strings. Defaults to a version of <see cref="Encoding.UTF8"/> that does not write a BOM.
	/// </summary>
	[JsonIgnore]
	public Encoding Encoding { get; set; } = DefaultEncoding;

	public override uint Size => (uint) Encoding.GetByteCount(Value) + 1;

	public override void Read(BinaryReader reader)
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

				if (terminatorIndex + totalBytesRead > MaxLength)
					// Too many bytes *would* be read if we tried to reach the terminator; we still need to bail
					break;

				builder.Append(Encoding.GetString(buffer[..terminatorIndex]));
				Value = builder.ToString();

				// Seek the stream back to the byte after the terminator
				reader.BaseStream.Seek(-( bytesRead - terminatorIndex - 1 ), SeekOrigin.Current);

				return;
			}

			if (bytesRead + totalBytesRead > MaxLength)
				// Too many bytes would be read if we continued; bail now
				break;

			// Flush our current buffer to the builder and keep reading
			builder.Append(Encoding.GetString(buffer));
			totalBytesRead += bytesRead;
		}

		throw new InvalidDataException("Encountered end-of-stream or too many bytes while reading a terminated string");
	}

	public override void Write(BinaryWriter writer)
	{
		using var textWriter = new StreamWriter(writer.BaseStream, Encoding, leaveOpen: true);

		textWriter.Write(Value);
		textWriter.Flush();
		writer.Write((byte) '\0'); // Write the terminator
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
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

				if (terminatorIndex + totalBytesRead > MaxLength)
					// Too many bytes *would* be read if we tried to reach the terminator; we still need to bail
					break;

				builder.Append(Encoding.GetString(buffer[..terminatorIndex]));
				Value = builder.ToString();

				// Seek the stream back to the byte after the terminator
				reader.BaseStream.Seek(-( bytesRead - terminatorIndex - 1 ), SeekOrigin.Current);

				return;
			}

			if (bytesRead + totalBytesRead > MaxLength)
				// Too many bytes would be read if we continued; bail now
				break;

			// Flush our current buffer to the builder and keep reading
			builder.Append(Encoding.GetString(buffer));
			totalBytesRead += bytesRead;
		}

		throw new InvalidDataException("Encountered end-of-stream or too many bytes while reading a terminated string");
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		var stream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
		await using var textWriter = new StreamWriter(stream, Encoding, leaveOpen: true);

		await textWriter.WriteAsync(Value).ConfigureAwait(false);
		await textWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync((byte) '\0', cancellationToken).ConfigureAwait(false); // Write the terminator
	}
}