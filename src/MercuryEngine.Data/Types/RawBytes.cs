using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types;

public class RawBytes(Func<Stream, int> getSizeForReading) : BaseBinaryField<byte[]>([])
{
	public RawBytes(Func<int> getSizeForReading)
		: this(_ => getSizeForReading()) { }

	public override uint Size => (uint) ( Value.Length );

	public override void Read(BinaryReader reader, ReadContext context)
	{
		var bytesToRead = getSizeForReading(reader.BaseStream);

		Value = new byte[bytesToRead];

		var bytesRead = reader.Read(Value, 0, bytesToRead);

		if (bytesRead != bytesToRead)
			throw new IOException($"Expected to read {bytesToRead} bytes, but only read {bytesRead}");
	}

	public override void Write(BinaryWriter writer, WriteContext context)
	{
		writer.Write(Value);
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		var bytesToRead = getSizeForReading(reader.BaseStream);

		Value = new byte[bytesToRead];

		var bytesRead = await reader.ReadAsync(Value, 0, bytesToRead, cancellationToken).ConfigureAwait(false);

		if (bytesRead != bytesToRead)
			throw new IOException($"Expected to read {bytesToRead} bytes, but only read {bytesRead}");
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Value, cancellationToken).ConfigureAwait(false);
	}
}