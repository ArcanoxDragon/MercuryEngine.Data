using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

public class PaddingField : IBinaryField
{
	private readonly byte[] buffer;

	public PaddingField(uint length, byte paddingByte = 0)
	{
		this.buffer = new byte[length];

		Array.Fill(this.buffer, paddingByte);
	}

	public uint Size => (uint) this.buffer.Length;

	public void Read(BinaryReader reader, ReadContext context)
	{
		var bytesRead = reader.Read(this.buffer);

		if (bytesRead < Size)
			throw new IOException($"Expected {Size} bytes of padding, but only got {bytesRead}");
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		writer.Write(this.buffer);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		var bytesRead = await reader.ReadAsync(this.buffer, 0, this.buffer.Length, cancellationToken).ConfigureAwait(false);

		if (bytesRead < Size)
			throw new IOException($"Expected {Size} bytes of padding, but only got {bytesRead}");
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(this.buffer, cancellationToken).ConfigureAwait(false);
	}
}