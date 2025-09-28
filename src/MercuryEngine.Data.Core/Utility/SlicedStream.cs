namespace MercuryEngine.Data.Core.Utility;

public class SlicedStream : Stream
{
	private readonly long basePosition;
	private readonly bool disposeInner;

	private long length;

	public SlicedStream(Stream baseStream, long start, long length, bool keepOpen = true)
	{
		if (!baseStream.CanRead)
			throw new ArgumentException("Base stream must be readable", nameof(baseStream));

		ArgumentOutOfRangeException.ThrowIfNegative(start);
		ArgumentOutOfRangeException.ThrowIfNegative(length);

		if (start + length > baseStream.Length)
			throw new ArgumentException("Slice cannot extend beyond the bounds of the base stream");

		BaseStream = baseStream;

		this.basePosition = start;
		this.disposeInner = !keepOpen;
		this.length = length;
	}

	public override long Length => this.length;

	public override bool CanRead  => true;
	public override bool CanSeek  => BaseStream.CanSeek;
	public override bool CanWrite => BaseStream.CanWrite;

	public override long Position
	{
		get => BaseStream.Position - this.basePosition;
		set => BaseStream.Position = value + this.basePosition;
	}

	internal Stream BaseStream { get; }

	internal bool HideRealPosition { get; set; }

	public override void Flush() { }

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (Position < 0)
			throw new InvalidOperationException($"Cannot read before the start of a {nameof(SlicedStream)}");

		return BaseStream.Read(buffer, offset, (int) Math.Min(count, Length - Position));
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		Position = origin switch {
			SeekOrigin.Current => Position + offset,
			SeekOrigin.End     => Length + offset,
			_                  => offset,
		};

		return Position;
	}

	public override void SetLength(long value) => throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count)
	{
		EnsureWriteable();
		BaseStream.Write(buffer, offset, count);
		this.length += count;
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		EnsureWriteable();
		BaseStream.Write(buffer);
		this.length += buffer.Length;
	}

	public override void WriteByte(byte value)
	{
		EnsureWriteable();
		BaseStream.WriteByte(value);
		this.length++;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing && this.disposeInner)
			BaseStream.Dispose();
	}

	public override ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return this.disposeInner ? BaseStream.DisposeAsync() : base.DisposeAsync();
	}

	private void EnsureWriteable()
	{
		if (!BaseStream.CanWrite)
			throw new NotSupportedException("Base stream is not writeable");
	}
}