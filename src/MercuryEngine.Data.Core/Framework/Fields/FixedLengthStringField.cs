using System.Text;
using JetBrains.Annotations;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class FixedLengthStringField(int length, string initialValue) : BaseBinaryField<string>(initialValue)
{
	public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

	private const int BufferSize = 2048;

	private static readonly byte[] PaddingBuffer = new byte[BufferSize];

	static FixedLengthStringField()
	{
		Array.Fill(PaddingBuffer, (byte) 0);
	}

	public FixedLengthStringField(int length) : this(length, string.Empty) { }

	public FixedLengthStringField(string value) : this(Encoding.UTF8.GetByteCount(value), value) { }

	/// <summary>
	/// Gets or sets the string value of this <see cref="FixedLengthStringField"/>.
	/// </summary>
	public override string Value
	{
		get => base.Value;
		set
		{
			ArgumentNullException.ThrowIfNull(value);

			if (Encoding.GetByteCount(value) > Length)
				throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be larger than the maximum length of {Length} bytes.");

			base.Value = value;
		}
	}

	/// <summary>
	/// The maximum length of a string that can be read or written by this component.
	/// Strings larger than this will throw an exception.
	/// </summary>
	public int Length { get; } = length;

	/// <summary>
	/// The encoding used when reading and writing strings. Defaults to a version of <see cref="Encoding.UTF8"/> that does not write a BOM.
	/// </summary>
	public Encoding Encoding { get; set; } = DefaultEncoding;

	public override uint Size => (uint) Length;

	public override void Read(BinaryReader reader)
	{
		var buffer = new byte[BufferSize];
		var builder = new StringBuilder();
		var bytesRemaining = Length;

		while (bytesRemaining > 0 && reader.BaseStream.Read(buffer, 0, Math.Min(bytesRemaining, buffer.Length)) is > 0 and var bytesRead)
		{
			var terminatorIndex = Array.IndexOf(buffer, (byte) '\0');

			if (terminatorIndex >= 0)
			{
				// Found a terminator!

				builder.Append(Encoding.GetString(buffer[..terminatorIndex]));
				Value = builder.ToString();
				return;
			}

			// Flush our current buffer to the builder and keep reading
			builder.Append(Encoding.GetString(buffer));
			bytesRemaining -= bytesRead;
		}

		if (bytesRemaining > 0)
		{
			var bytesRead = Length - bytesRemaining;

			throw new InvalidDataException($"Encountered end-of-stream while reading a fixed-length string " +
										   $"(wanted {Length} bytes, but only read {bytesRead})");
		}
	}

	public override void Write(BinaryWriter writer)
	{
		using var textWriter = new StreamWriter(writer.BaseStream, Encoding, leaveOpen: true);

		textWriter.Write(Value);
		textWriter.Flush();

		// Write 0s to fill the alotted space
		var byteCount = Encoding.GetByteCount(Value);
		var paddingNeeded = Length - byteCount;

		while (paddingNeeded > 0)
		{
			writer.Write(PaddingBuffer, 0, paddingNeeded);
			paddingNeeded -= PaddingBuffer.Length;
		}
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[BufferSize];
		var builder = new StringBuilder();
		var bytesRemaining = Length;

		Task<int> ReadChunkAsync()
			=> reader.BaseStream.ReadAsync(buffer, 0, Math.Min(bytesRemaining, buffer.Length), cancellationToken);

		while (bytesRemaining > 0 && await ReadChunkAsync().ConfigureAwait(false) is > 0 and var bytesRead)
		{
			var terminatorIndex = Array.IndexOf(buffer, (byte) '\0');

			if (terminatorIndex >= 0)
			{
				// Found a terminator!

				builder.Append(Encoding.GetString(buffer[..terminatorIndex]));
				Value = builder.ToString();
				return;
			}

			// Flush our current buffer to the builder and keep reading
			builder.Append(Encoding.GetString(buffer));
			bytesRemaining -= bytesRead;
		}

		if (bytesRemaining > 0)
		{
			var bytesRead = Length - bytesRemaining;

			throw new InvalidDataException($"Encountered end-of-stream while reading a fixed-length string " +
										   $"(wanted {Length} bytes, but only read {bytesRead})");
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		var stream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
		await using var textWriter = new StreamWriter(stream, Encoding, leaveOpen: true);

		await textWriter.WriteAsync(Value).ConfigureAwait(false);
		await textWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

		// Write 0s to fill the alotted space
		var byteCount = Encoding.GetByteCount(Value);
		var paddingNeeded = Length - byteCount;

		while (paddingNeeded > 0)
		{
			await writer.WriteAsync(PaddingBuffer, 0, paddingNeeded, cancellationToken).ConfigureAwait(false);
			paddingNeeded -= PaddingBuffer.Length;
		}
	}
}