using System.Text;

namespace MercuryEngine.Data.Framework.Components;

public class TerminatedStringComponent : BinaryComponent<string>
{
	private const int BufferSize       = 2048;
	private const int DefaultMaxLength = 1024 * 8; // 8 kB maximum by default

	/// <summary>
	/// The maximum length of a string that can be read or written by this component.
	/// Strings larger than this will throw an exception. Defaults to 8kb (8192 ASCII characters).
	/// </summary>
	public int MaxLength { get; set; } = DefaultMaxLength;

	/// <summary>
	/// The encoding used when reading and writing strings. Defaults to <see cref="System.Text.Encoding.UTF8"/>.
	/// </summary>
	public Encoding Encoding { get; set; } = Encoding.UTF8;

	public override bool IsFixedSize => false;

	public override bool Validate(Stream stream)
		=> true;

	public override string Read(BinaryReader reader)
	{
		var buffer = new byte[BufferSize];
		var builder = new StringBuilder();
		var totalBytesRead = 0;

		while (reader.BaseStream.Read(buffer, 0, buffer.Length) is > 0 and { } bytesRead)
		{
			var terminatorIndex = Array.IndexOf(buffer, (byte) '\0');

			if (terminatorIndex >= 0)
			{
				// Found a terminator!

				if (terminatorIndex + totalBytesRead > MaxLength)
					// Too many bytes *would* be read if we tried to reach the terminator; we still need to bail
					break;

				builder.Append(Encoding.GetString(buffer[..terminatorIndex]));
				return builder.ToString();
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

	public override void Write(BinaryWriter writer, string data)
	{
		var byteCount = Encoding.GetByteCount(data);

		if (byteCount >= MaxLength)
			throw new ArgumentOutOfRangeException(nameof(data), $"The input string was larger than the maximum size of {MaxLength} bytes");

		using var textWriter = new StreamWriter(writer.BaseStream, Encoding, leaveOpen: true);

		textWriter.Write(data);
		writer.Write((byte) '\0'); // Write the terminator
	}
}