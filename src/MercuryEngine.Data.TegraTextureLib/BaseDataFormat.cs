using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib;

public abstract class BaseDataFormat
{
	private protected BaseDataFormat() { }

	public async Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		await ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public abstract Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default);
}