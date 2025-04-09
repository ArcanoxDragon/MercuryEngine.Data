using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using Overby.Extensions.AsyncBinaryReaderWriter;

#if DEBUG
using MercuryEngine.Data.Core.Utility;
#endif

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class TerminatedStringField(string initialValue) : BaseBinaryField<string>(initialValue)
{
	public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

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
		Value = reader.ReadTerminatedCString(Encoding, MaxLength);
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteTerminatedCString(Encoding, Value);
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Value = await reader.ReadTerminatedCStringAsync(Encoding, MaxLength, cancellationToken).ConfigureAwait(false);
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteTerminatedCStringAsync(Encoding, Value, cancellationToken).ConfigureAwait(false);
	}
}