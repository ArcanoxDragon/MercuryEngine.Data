using System.Text;
using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.DataTypes;

[PublicAPI]
public class TerminatedStringDataType : BaseDataType<string>
{
	private const int BufferSize       = 2048;
	private const int DefaultMaxLength = 1024 * 8; // 8 kB maximum by default

	public TerminatedStringDataType() : this(string.Empty) { }

	public TerminatedStringDataType(string initialValue) : base(initialValue) { }

	/// <summary>
	/// Gets or sets the string value of this <see cref="TerminatedStringDataType"/>.
	/// </summary>
	public override string Value
	{
		get => base.Value;
		set
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			if (Encoding.GetByteCount(value) > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(value), $"Value cannot be larger than the maximum length of {MaxLength} bytes.");

			base.Value = value;
		}
	}

	/// <summary>
	/// The maximum length of a string that can be read or written by this component.
	/// Strings larger than this will throw an exception. Defaults to 8kb (8192 ASCII characters).
	/// </summary>
	public int MaxLength { get; set; } = DefaultMaxLength;

	/// <summary>
	/// The encoding used when reading and writing strings. Defaults to <see cref="System.Text.Encoding.UTF8"/>.
	/// </summary>
	public Encoding Encoding { get; set; } = Encoding.UTF8;

	public override uint Size => (uint) Encoding.GetByteCount(Value);

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
}