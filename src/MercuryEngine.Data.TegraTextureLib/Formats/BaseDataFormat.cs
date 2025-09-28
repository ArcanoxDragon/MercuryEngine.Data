using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public abstract class BaseDataFormat
{
	private protected BaseDataFormat() { }

	#region Synchronous

	public void Read(Stream stream)
	{
		using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		Read(reader);
	}

	public abstract void Read(BinaryReader reader);

	public void Write(Stream stream)
	{
		using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

		Write(writer);
	}

	public abstract void Write(BinaryWriter writer);

	#endregion

	#region Asynchronous

	public async Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		await ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public abstract Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default);

	public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		using var writer = new AsyncBinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

		await WriteAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	public abstract Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default);

	#endregion
}