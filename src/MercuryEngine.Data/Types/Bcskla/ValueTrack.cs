using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bcskla;

public class ValueTrack : IBinaryField
{
	private static readonly byte[] PaddingBuffer = [0xFF, 0xFF, 0xFF, 0xFF];

	public int Count
	{
		get;
		set
		{
			field = value;
			ResizeArrays();
		}
	}

	public ushort[]        Timings { get; private set; } = [];
	public KeyframeValue[] Values  { get; private set; } = [];

	public uint GetSize(uint startPosition)
	{
		var totalSize = (uint) (
			sizeof(ushort) + // TimingType
			sizeof(ushort) + // Count
			( Count * GetTimingValueSize() )
		);

		totalSize += MathHelper.GetNeededPaddingForAlignment(startPosition + totalSize, 4);

		if (Count == 0)
			return totalSize;

		// We cheat here - size of KeyframeValue is not dependent on position, so we can just use first
		var valueSize = Values[0].GetSize(0);

		totalSize += (uint) ( Count * valueSize );

		return totalSize;
	}

	public void Read(BinaryReader reader, ReadContext context)
	{
		var timingType = (TimingType) reader.ReadUInt16();

		Count = reader.ReadUInt16();

		for (var i = 0; i < Count; i++)
		{
			if (timingType == TimingType.TwoBytes)
				Timings[i] = reader.ReadUInt16();
			else
				Timings[i] = reader.ReadByte();
		}

		var neededPadding = (int) reader.BaseStream.GetNeededPaddingForAlignment(4);

		reader.ReadBytes(neededPadding);

		for (var i = 0; i < Count; i++)
			Values[i].Read(reader, context);
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		var timingType = GetTimingType();

		writer.Write((ushort) timingType);
		writer.Write((ushort) Count);

		for (var i = 0; i < Count; i++)
		{
			if (timingType == TimingType.TwoBytes)
				writer.Write(Timings[i]);
			else
				writer.Write((byte) Timings[i]);
		}

		var neededPadding = (int) writer.BaseStream.GetNeededPaddingForAlignment(4);

		writer.Write(PaddingBuffer[..neededPadding]);

		for (var i = 0; i < Count; i++)
			Values[i].Write(writer, context);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		var timingType = (TimingType) await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);

		Count = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < Count; i++)
		{
			if (timingType == TimingType.TwoBytes)
				Timings[i] = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
			else
				Timings[i] = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
		}

		var neededPadding = reader.BaseStream.GetNeededPaddingForAlignment(4);

		await reader.ReadBytesAsync((int) neededPadding, cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < Count; i++)
			await Values[i].ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		var timingType = GetTimingType();

		await writer.WriteAsync((ushort) timingType, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync((ushort) Count, cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < Count; i++)
		{
			if (timingType == TimingType.TwoBytes)
				await writer.WriteAsync(Timings[i], cancellationToken).ConfigureAwait(false);
			else
				await writer.WriteAsync((byte) Timings[i], cancellationToken).ConfigureAwait(false);
		}

		var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
		var neededPadding = (int) baseStream.GetNeededPaddingForAlignment(4);

		await writer.WriteAsync(PaddingBuffer[..neededPadding], cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < Count; i++)
			await Values[i].WriteAsync(writer, context, cancellationToken).ConfigureAwait(false);
	}

	private int GetTimingValueSize()
		=> GetTimingType() == TimingType.TwoBytes ? sizeof(ushort) : sizeof(byte);

	private TimingType GetTimingType()
	{
		for (var i = 0; i < Count; i++)
		{
			if (Timings[i] > 0xFF)
				return TimingType.TwoBytes;
		}

		return TimingType.OneByte;
	}

	private void ResizeArrays()
	{
		if (Timings.Length != Count)
		{
			var newTimings = new ushort[Count];

			Array.Copy(Timings, newTimings, Math.Min(Timings.Length, newTimings.Length));
			Timings = newTimings;
		}

		if (Values.Length != Count)
		{
			var newValues = new KeyframeValue[Count];

			Array.Copy(Values, newValues, Math.Min(Values.Length, newValues.Length));
			Values = newValues;
		}
	}

	private enum TimingType : ushort
	{
		OneByte  = 8,
		TwoBytes = 0,
	}
}