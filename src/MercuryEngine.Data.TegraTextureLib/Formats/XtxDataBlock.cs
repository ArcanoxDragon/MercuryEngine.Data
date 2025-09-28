using System.Buffers;
using System.Text;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public sealed class XtxDataBlock : BaseDataFormat
{
	private const string Signature = "HBvN"; // NvBH, but stored as a little endian uint32 for some reason?

	private byte[] data = [];

	#region Data Fields

	/// <summary>
	/// The size of this structure, not including its data.
	/// </summary>
	public uint BlockHeaderSize { get; private set; } = (uint) (
		Signature.Length +
		sizeof(uint) +  // BlockSize
		sizeof(ulong) + // DataSize
		sizeof(long) +  // DataOffset
		sizeof(uint) +  // BlockType
		sizeof(uint) +  // GlobalBlockIndex
		sizeof(uint)    // IncBlockTypeIndex
	);

	public  ulong        DataSize          { get; private set; }
	private long         DataOffset        { get; set; }
	public  XtxBlockType BlockType         { get; set; }
	public  uint         GlobalBlockIndex  { get; set; }
	public  uint         IncBlockTypeIndex { get; set; }

	#endregion

	/// <summary>
	/// Gets or sets the byte-alignment used when writing the data block.
	/// </summary>
	public uint DataAlignment { get; set; } = 0;

	public byte[] Data
	{
		get => this.data;
		set
		{
			this.data = value;
			DataSize = (ulong) value.LongLength;
		}
	}

	#region Synchronous

	public override void Read(BinaryReader reader)
	{
		var startPosition = reader.BaseStream.Position;
		var signature = reader.ReadBytes(4);

		if (Encoding.ASCII.GetString(signature) != Signature)
			throw new IOException("Signature mismatch: not a valid XTX data block");

		BlockHeaderSize = reader.ReadUInt32();
		DataSize = reader.ReadUInt64();
		DataOffset = reader.ReadInt64();
		BlockType = (XtxBlockType) reader.ReadUInt32();
		GlobalBlockIndex = reader.ReadUInt32();  // TODO: ???
		IncBlockTypeIndex = reader.ReadUInt32(); // TODO: ???

		reader.BaseStream.Seek(startPosition + DataOffset, SeekOrigin.Begin);
		this.data = reader.ReadBytes((int) DataSize);
	}

	public override void Write(BinaryWriter writer)
	{
		CalculateDataOffset(writer.BaseStream, out var neededPadding);

		writer.Write(Encoding.ASCII.GetBytes(Signature));
		writer.Write(BlockHeaderSize);
		writer.Write(DataSize);
		writer.Write(DataOffset);
		writer.Write((uint) BlockType);
		writer.Write(GlobalBlockIndex);
		writer.Write(IncBlockTypeIndex);

		Span<byte> paddingBuffer = stackalloc byte[(int) neededPadding];

		paddingBuffer.Clear();

		writer.Write(paddingBuffer);
		writer.Write(Data);
	}

	#endregion

	#region Asynchronous

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		var startPosition = reader.BaseStream.Position;
		var signature = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);

		if (Encoding.ASCII.GetString(signature) != Signature)
			throw new IOException("Signature mismatch: not a valid XTX data block");

		BlockHeaderSize = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		DataSize = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);
		DataOffset = await reader.ReadInt64Async(cancellationToken).ConfigureAwait(false);
		BlockType = (XtxBlockType) await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		GlobalBlockIndex = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		IncBlockTypeIndex = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		reader.BaseStream.Seek(startPosition + DataOffset, SeekOrigin.Begin);
		this.data = await reader.ReadBytesAsync((int) DataSize, cancellationToken).ConfigureAwait(false);
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

		CalculateDataOffset(baseStream, out var neededPadding);

		await writer.WriteAsync(Encoding.ASCII.GetBytes(Signature), cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(BlockHeaderSize, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(DataSize, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(DataOffset, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync((uint) BlockType, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(GlobalBlockIndex, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(IncBlockTypeIndex, cancellationToken).ConfigureAwait(false);

		// No Memory<T> support on AsyncBinaryWriter, so we need to use the stream directly
		baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

		var paddingSize = (int) neededPadding;
		using var paddingMemory = MemoryPool<byte>.Shared.Rent(paddingSize);

		await baseStream.WriteAsync(paddingMemory.Memory[..paddingSize], cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Data, cancellationToken).ConfigureAwait(false);
	}

	#endregion

	private void CalculateDataOffset(Stream baseStream, out uint paddingBytes)
	{
		if (DataAlignment == 0)
		{
			paddingBytes = 0;
			DataOffset = BlockHeaderSize; // No alignment constraint - data starts immediately after header
			return;
		}

		var originalDataStartPosition = (uint) ( baseStream.Position + BlockHeaderSize );
		var neededPadding = MathHelper.GetNeededPaddingForAlignment(originalDataStartPosition, DataAlignment);
		var newDataStartPosition = originalDataStartPosition + neededPadding;

		// DataOffset is relative to the start of the block, so we need to subtract the stream position again
		DataOffset = newDataStartPosition - baseStream.Position;
		paddingBytes = neededPadding;
	}
}