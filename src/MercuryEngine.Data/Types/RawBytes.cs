using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types;

public class RawBytes(Func<Stream, int> getSizeForReading) : BaseBinaryField<byte[]>([])
{
	public RawBytes(Func<int> getSizeForReading)
		: this(_ => getSizeForReading()) { }

	public override uint Size
	{
		get
		{
			var neededPadding = GetNeededPadding(Value.Length);

			return (uint) ( Value.Length + neededPadding );
		}
	}

	/*public uint ByteAlignment { get; set; }
	public byte PaddingByte   { get; set; }*/

	public override void Read(BinaryReader reader, ReadContext context)
	{
		var bytesToRead = getSizeForReading(reader.BaseStream);

		Value = new byte[bytesToRead];

		var bytesRead = reader.Read(Value, 0, bytesToRead);

		if (bytesRead != bytesToRead)
			throw new IOException($"Expected to read {bytesToRead} bytes, but only read {bytesRead}");

		/*if (ByteAlignment > 0)
		{
			var neededPadding = GetNeededPadding(reader.BaseStream.Position);

			reader.BaseStream.Seek(neededPadding, SeekOrigin.Current);
		}*/
	}

	public override void Write(BinaryWriter writer, WriteContext context)
	{
		writer.Write(Value);

		/*if (ByteAlignment > 0)
		{
			var neededPadding = GetNeededPadding(writer.BaseStream.Position);

			for (var i = 0; i < neededPadding; i++)
				writer.Write(PaddingByte);
		}*/
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		var bytesToRead = getSizeForReading(reader.BaseStream);

		Value = new byte[bytesToRead];

		var bytesRead = await reader.ReadAsync(Value, 0, bytesToRead, cancellationToken).ConfigureAwait(false);

		if (bytesRead != bytesToRead)
			throw new IOException($"Expected to read {bytesToRead} bytes, but only read {bytesRead}");

		/*if (ByteAlignment > 0)
		{
			var neededPadding = GetNeededPadding(reader.BaseStream.Position);

			reader.BaseStream.Seek(neededPadding, SeekOrigin.Current);
		}*/
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Value, cancellationToken).ConfigureAwait(false);

		/*if (ByteAlignment > 0)
		{
			var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
			var neededPadding = GetNeededPadding(baseStream.Position);

			for (var i = 0; i < neededPadding; i++)
				await writer.WriteAsync(PaddingByte, cancellationToken).ConfigureAwait(false);
		}*/
	}

	private uint GetNeededPadding(long currentPosition)
	{
		return 0;
		/*var misalignment = currentPosition % ByteAlignment;

		return misalignment == 0 ? 0 : (uint) ( ByteAlignment - misalignment );*/
	}
}