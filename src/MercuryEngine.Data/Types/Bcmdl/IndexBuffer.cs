using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Types.Bcmdl;

public class IndexBuffer : DataStructure<IndexBuffer>
{
	private bool dataChanged;

	public ulong  UnknownPointer   { get; set; }
	public uint   IndexCount       { get; private set; }
	public uint   CompressedSize   { get; private set; }
	public byte[] UncompressedData { get; private set; } = [];

	public bool IsCompressed
	{
		get;
		set
		{
			field = value;
			this.dataChanged = true;
		}
	}

	public ushort[] GetIndices()
	{
		var indices = new ushort[IndexCount];
		var sourceBytes = UncompressedData.AsSpan();
		var destBytes = MemoryMarshal.Cast<ushort, byte>(indices.AsSpan());

		sourceBytes.CopyTo(destBytes);

		return indices;
	}

	public void ReplaceIndices(ReadOnlySpan<ushort> indices)
	{
		if (UncompressedData.Length != indices.Length * sizeof(ushort))
			UncompressedData = new byte[indices.Length * sizeof(ushort)];

		var sourceBytes = MemoryMarshal.Cast<ushort, byte>(indices);
		var destBytes = UncompressedData;

		sourceBytes.CopyTo(destBytes);
		IndexCount = (uint) indices.Length;
		this.dataChanged = true;
	}

	#region Private Data

	private RawBytes? BufferData { get; set; }

	private byte[] RawData
	{
		get => BufferData?.Value ?? [];
		set
		{
			BufferData ??= CreateRawDataField();
			BufferData.Value = value;
			CompressedSize = (uint) value.Length;
		}
	}

	#endregion

	#region Hooks

	protected override void AfterRead(ReadContext context)
	{
		base.AfterRead(context);

		IsCompressed = GzipHelper.IsDataCompressed(RawData);

		if (IsCompressed)
			UncompressedData = GzipHelper.DecompressData(RawData);
		else
			UncompressedData = RawData;

		this.dataChanged = false;
	}

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		if (this.dataChanged)
		{
			if (IsCompressed)
				RawData = GzipHelper.CompressData(UncompressedData);
			else
				RawData = UncompressedData;

			CompressedSize = (uint) RawData.Length;
		}
	}

	protected override void AfterWrite(WriteContext context)
	{
		base.AfterWrite(context);

		this.dataChanged = false;
	}

	#endregion

	private int GetBufferDataLength(Stream stream)
	{
		var headerPeek = stream.Peek(8);
		var isCompressed = GzipHelper.IsDataCompressed(headerPeek);

		if (isCompressed)
			return (int) CompressedSize;

		// Uncompressed data is equal to 2 * IndexCount, as the indices are all uint16 values
		return (int) ( 2 * IndexCount );
	}

	private RawBytes CreateRawDataField()
		=> new(GetBufferDataLength);

	protected override void Describe(DataStructureBuilder<IndexBuffer> builder)
	{
		builder.Property(m => m.UnknownPointer);
		builder.Constant(0x00010002, "<version>");
		builder.Property(m => m.IndexCount);
		builder.Property(m => m.CompressedSize);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.BufferData, owner => owner.CreateRawDataField(), startByteAlignment: 8, endByteAlignment: 8);
	}
}