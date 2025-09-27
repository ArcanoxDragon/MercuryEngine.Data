using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public abstract class BaseDataFormat
{
	private protected BaseDataFormat() { }

	public void Read(Stream stream)
	{
		using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		Read(reader);
	}

	public async Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		await ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public abstract void Read(BinaryReader reader);

	public abstract Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default);
}