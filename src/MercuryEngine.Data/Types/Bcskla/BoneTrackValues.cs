using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bcskla;

public class BoneTrackValues : IBinaryField
{
	private readonly AnimatableValue[] valuesArray;

	public BoneTrackValues()
	{
		// For enumeration through all 9 values
		this.valuesArray = [
			Position.X, Position.Y, Position.Z,
			Rotation.X, Rotation.Y, Rotation.Z,
			Scale.X, Scale.Y, Scale.Z,
		];
	}

	public AnimatableVector Position { get; } = new();
	public AnimatableVector Rotation { get; } = new();
	public AnimatableVector Scale    { get; } = new();

	public uint GetSize(uint startPosition)
	{
		var totalSize = (uint) sizeof(uint);

		totalSize += Position.GetSize(startPosition + totalSize);
		totalSize += Rotation.GetSize(startPosition + totalSize);
		totalSize += Scale.GetSize(startPosition + totalSize);

		return totalSize;
	}

	public void Read(BinaryReader reader, ReadContext context)
	{
		var flags = reader.ReadUInt32();

		SetPointerBases(reader.BaseStream.Position);

		for (var i = 0; i < this.valuesArray.Length; i++)
		{
			var value = this.valuesArray[i];
			var isKeyframed = ( flags & ( 1 << i ) ) > 0;

			value.IsConstant = !isKeyframed;
			value.Read(reader, context);
		}
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		writer.Write(GetFlags());

		SetPointerBases(writer.BaseStream.Position);

		foreach (var value in this.valuesArray)
			value.Write(writer, context);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		var flags = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		SetPointerBases(reader.BaseStream.Position);

		for (var i = 0; i < 9; i++)
		{
			var value = this.valuesArray[i];
			var isKeyframed = ( flags & ( 1 << i ) ) > 0;

			value.IsConstant = !isKeyframed;
			await value.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(GetFlags(), cancellationToken).ConfigureAwait(false);

		var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

		SetPointerBases(baseStream.Position);

		foreach (var value in this.valuesArray)
			await value.WriteAsync(writer, context, cancellationToken).ConfigureAwait(false);
	}

	private uint GetFlags()
	{
		var flags = 0u;

		for (var i = 0; i < this.valuesArray.Length; i++)
		{
			if (!this.valuesArray[i].IsConstant)
				flags |= 1u << i;
		}

		return flags;
	}

	private void SetPointerBases(long streamPosition)
	{
		var pointerBase = (ulong) streamPosition;

		foreach (var value in this.valuesArray)
			value.PointerBase = pointerBase;
	}
}