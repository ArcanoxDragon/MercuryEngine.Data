using System.IO.Compression;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bcmdl.Wrappers;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Types.Bcmdl;

public class VertexBuffer : DataStructure<VertexBuffer>
{
	private bool dataChanged;

	public ulong UnknownPointer   { get; set; }
	public uint  UnknownValue     { get; set; }
	public uint  UncompressedSize { get; private set; }
	public uint  VertexCount      { get; private set; }
	public uint  CompressedSize   { get; private set; }

	public List<VertexInfoDescription> VertexInfoSlots => VertexInfoSlotsList.Entries;

	public byte[] UncompressedData { get; private set; } = [];
	public bool   IsCompressed     { get; set; }

	/// <summary>
	/// Deconstructs the raw vertex buffer data into an array of <see cref="VertexData"/> objects
	/// that can be used to more easily read vertex information.
	/// </summary>
	public VertexData[] GetVertices()
	{
		var stride = VertexInfoSlots.Sum(slot => slot.Count * sizeof(float)); // All components are expected to be float-based vectors for Dread

		if (UncompressedData.Length != ( stride * VertexCount ))
			throw new InvalidOperationException($"Vertex buffer data must be {stride * VertexCount} bytes ({VertexCount} vertices * {stride} bytes per vertex), but it was {UncompressedData.Length}");

		var dataSpan = UncompressedData.AsSpan();
		var vertices = new VertexData[VertexCount];
		var firstSlot = true;

		foreach (var infoSlot in VertexInfoSlots)
		{
			for (var i = 0u; i < VertexCount; i++)
			{
				if (firstSlot)
					vertices[i] = new VertexData();

				vertices[i].ReadFromVertexBuffer(dataSpan, infoSlot, i);
			}

			firstSlot = false;
		}

		return vertices;
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

	private VertexInfoList VertexInfoSlotsList { get; } = new();

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

		return (int) ( isCompressed ? CompressedSize : UncompressedSize );
	}

	private RawBytes CreateRawDataField()
		=> new(GetBufferDataLength);

	protected override void Describe(DataStructureBuilder<VertexBuffer> builder)
	{
		builder.Property(m => m.UnknownPointer);
		builder.Property(m => m.UnknownValue);
		builder.Property(m => m.UncompressedSize);
		builder.Property(m => m.VertexCount);
		builder.Property(m => m.CompressedSize);
		builder.Pointer(m => m.BufferData, owner => owner.CreateRawDataField(), startByteAlignment: 8, endByteAlignment: 8);
		builder.RawProperty(m => m.VertexInfoSlotsList);
	}
}