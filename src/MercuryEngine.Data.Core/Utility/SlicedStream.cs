﻿namespace MercuryEngine.Data.Core.Utility;

public class SlicedStream : Stream
{
	private readonly long basePosition;

	public SlicedStream(Stream baseStream, long start, long length)
	{
		if (!baseStream.CanRead)
			throw new ArgumentException("Base stream must be readable", nameof(baseStream));

		ArgumentOutOfRangeException.ThrowIfNegative(start);
		ArgumentOutOfRangeException.ThrowIfNegative(length);

		if (start + length > baseStream.Length)
			throw new ArgumentException("Slice cannot extend beyond the bounds of the base stream");

		Length = length;
		BaseStream = baseStream;

		this.basePosition = start;
	}

	public override long Length { get; }

	public override bool CanRead  => true;
	public override bool CanSeek  => BaseStream.CanSeek;
	public override bool CanWrite => false;

	public override long Position
	{
		get => BaseStream.Position - this.basePosition;
		set => BaseStream.Position = value + this.basePosition;
	}

	internal Stream BaseStream { get; }

	public override void Flush() { }

	public override int Read(byte[] buffer, int offset, int count)
		=> BaseStream.Read(buffer, offset, (int) Math.Min(count, Length - Position));

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

	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}