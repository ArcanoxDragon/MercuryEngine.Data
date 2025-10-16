using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bcskla;

public class AnimatableValue : IBinaryField
{
	private readonly List<(ushort, KeyframeValue)> keyframeValues = [];

	public bool IsConstant { get; internal set; }

	/// <summary>
	/// Gets the value of the <see cref="AnimatableValue"/> if it is constant.
	/// </summary>
	/// <remarks>
	/// Returns <see langword="default"/> if the value is not constant.
	/// </remarks>
	public float ConstantValue
	{
		get => IsConstant ? field : 0f;
		set
		{
			field = value;
			IsConstant = true;
			this.keyframeValues.Clear();
		}
	}

	public int ValueCount => IsConstant ? 1 : this.keyframeValues.Count;

	internal ulong PointerBase { get; set; }

	private ValueTrack ValueTrack { get; } = new();

	public void ClearValues()
	{
		IsConstant = false;
		this.keyframeValues.Clear();
	}

	public void AddValue(ushort frame, float value, float rate = 0f)
		=> AddValue(frame, new KeyframeValue(value, rate));

	public void AddValue(ushort frame, KeyframeValue value)
	{
		IsConstant = false;
		this.keyframeValues.Add(( frame, value ));
	}

	public IEnumerable<(ushort Frame, KeyframeValue Value)> GetValues()
	{
		if (IsConstant)
		{
			yield return ( 0, new KeyframeValue(ConstantValue, 0f) );
		}
		else
		{
			foreach (var entry in this.keyframeValues)
				yield return entry;
		}
	}

	#region IBinaryField

	public uint GetSize(uint startPosition) => sizeof(uint); // Float is the same size in case we're constant

	public void Read(BinaryReader reader, ReadContext context)
	{
		if (IsConstant)
		{
			ConstantValue = reader.ReadSingle();
			return;
		}

		var relativePointer = reader.ReadUInt32();
		var absolutePointer = PointerBase + relativePointer;
		var originalPosition = reader.BaseStream.Position;

		reader.BaseStream.Position = (long) absolutePointer;
		ValueTrack.Read(reader, context);
		context.HeapManager.Register(absolutePointer, ValueTrack);
		reader.BaseStream.Position = originalPosition;

		PopulateFromValueTrack();
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		if (IsConstant)
		{
			writer.Write(ConstantValue);
			return;
		}

		PopulateValueTrack();

		var absolutePointer = context.HeapManager.Allocate(ValueTrack);
		var relativePointer = (uint) ( absolutePointer - PointerBase );

		writer.Write(relativePointer);
		// ValueTrack will be written by HeapManager
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		if (IsConstant)
		{
			ConstantValue = await reader.ReadSingleAsync(cancellationToken).ConfigureAwait(false);
			return;
		}

		var relativePointer = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		var absolutePointer = PointerBase + relativePointer;
		var originalPosition = reader.BaseStream.Position;

		reader.BaseStream.Position = (long) absolutePointer;
		await ValueTrack.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		context.HeapManager.Register(absolutePointer, ValueTrack);
		reader.BaseStream.Position = originalPosition;

		PopulateFromValueTrack();
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		if (IsConstant)
		{
			await writer.WriteAsync(ConstantValue, cancellationToken).ConfigureAwait(false);
			return;
		}

		PopulateValueTrack();

		var absolutePointer = context.HeapManager.Allocate(ValueTrack);
		var relativePointer = (uint) ( absolutePointer - PointerBase );

		await writer.WriteAsync(relativePointer, cancellationToken).ConfigureAwait(false);
		// ValueTrack will be written by HeapManager
	}

	#endregion

	private void PopulateFromValueTrack()
	{
		this.keyframeValues.Clear();
		this.keyframeValues.AddRange(ValueTrack.Timings.Zip(ValueTrack.Values));
	}

	private void PopulateValueTrack()
	{
		ValueTrack.Count = this.keyframeValues.Count;

		for (var i = 0; i < this.keyframeValues.Count; i++)
		{
			var (frame, keyframeValue) = this.keyframeValues[i];

			ValueTrack.Timings[i] = frame;
			ValueTrack.Values[i] = keyframeValue;
		}
	}
}