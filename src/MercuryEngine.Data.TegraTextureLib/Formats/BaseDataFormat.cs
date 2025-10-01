using System.Text;
using MercuryEngine.Data.Core.Framework;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public abstract class BaseDataFormat : IBinaryFormat
{
	private protected BaseDataFormat() { }

	#region Synchronous

	public void Read(Stream stream)
		=> Read(stream, Encoding.UTF8);

	public void Read(Stream stream, Encoding encoding)
	{
		using var reader = new BinaryReader(stream, encoding, leaveOpen: true);

		Read(reader);
	}

	public abstract void Read(BinaryReader reader);

	public void Write(Stream stream)
		=> Write(stream, Encoding.UTF8);

	public void Write(Stream stream, Encoding encoding)
	{
		using var writer = new BinaryWriter(stream, encoding, leaveOpen: true);

		Write(writer);
	}

	public abstract void Write(BinaryWriter writer);

	#endregion

	#region Asynchronous

	public Task ReadAsync(Stream stream, CancellationToken cancellationToken = default)
		=> ReadAsync(stream, Encoding.UTF8, cancellationToken);

	public async Task ReadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, encoding, leaveOpen: true);

		await ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public abstract Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default);

	public Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
		=> WriteAsync(stream, Encoding.UTF8, cancellationToken);

	public async Task WriteAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
	{
		using var writer = new AsyncBinaryWriter(stream, encoding, leaveOpen: true);

		await WriteAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	public abstract Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default);

	#endregion
}