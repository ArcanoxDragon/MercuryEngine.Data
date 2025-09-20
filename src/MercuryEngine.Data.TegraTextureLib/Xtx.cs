using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;
using MercuryEngine.Data.TegraTextureLib.Extensions;

namespace MercuryEngine.Data.TegraTextureLib;

public class Xtx : BaseDataFormat
{
	private const string Signature = "DFvN";

	private const int BlockTypeTextureInfo = 2;
	private const int BlockTypeTextureData = 3;

	public uint HeaderSize   { get; private set; }
	public uint MajorVersion { get; private set; }
	public uint MinorVersion { get; private set; }

	public List<TegraTexture> Textures { get; } = [];

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Textures.Clear();

		var signature = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);

		if (Encoding.ASCII.GetString((byte[]) signature) != Signature)
			throw new IOException("Signature mismatch: not a valid XTX file");

		HeaderSize = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		MajorVersion = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		MinorVersion = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		reader.BaseStream.Seek(HeaderSize, SeekOrigin.Begin);

		// Read all data blocks
		var blocks = new List<DataBlock>();
		var textureDatas = new List<byte[]>();

		while (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			DataBlock block = new();

			await block.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			blocks.Add(block);

			if (block.BlockType == BlockTypeTextureData)
				textureDatas.Add(block.Data);
		}

		// Read TextureInfo block datas
		foreach (var (i, textureInfoBlock) in blocks.Where(b => b.BlockType == BlockTypeTextureInfo).Pairs())
		{
			if (i >= textureDatas.Count)
				throw new InvalidDataException($"No texture data block found for texture {i}");

			using var textureInfoStream = new MemoryStream(textureInfoBlock.Data);
			TextureInfo textureInfo = new();
			var textureData = textureDatas[i];

			await textureInfo.ReadAsync(textureInfoStream, cancellationToken).ConfigureAwait(false);

			Textures.Add(new TegraTexture(textureInfo, textureData));
		}
	}

	public sealed class DataBlock : BaseDataFormat
	{
		private const string Signature = "HBvN";

		public  uint  BlockSize         { get; private set; }
		public  ulong DataSize          { get; private set; }
		private long  DataOffset        { get; set; }
		public  uint  BlockType         { get; private set; }
		public  uint  GlobalBlockIndex  { get; private set; }
		public  uint  IncBlockTypeIndex { get; private set; }

		public byte[] Data { get; private set; } = [];

		public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		{
			var startPosition = reader.BaseStream.Position;

			var signature = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);

			if (Encoding.ASCII.GetString((byte[]) signature) != Signature)
				throw new IOException("Signature mismatch: not a valid XTX data block");

			BlockSize = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			DataSize = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);
			DataOffset = await reader.ReadInt64Async(cancellationToken).ConfigureAwait(false);
			BlockType = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			GlobalBlockIndex = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			IncBlockTypeIndex = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

			reader.BaseStream.Seek(startPosition + DataOffset, SeekOrigin.Begin);
			Data = await reader.ReadBytesAsync((int) DataSize, cancellationToken).ConfigureAwait(false);
		}
	}

	public sealed class TextureInfo : BaseDataFormat
	{
		public ulong          DataSize       { get; private set; }
		public uint           Alignment      { get; private set; }
		public uint           Width          { get; private set; }
		public uint           Height         { get; private set; }
		public uint           Depth          { get; private set; }
		public uint           Target         { get; private set; }
		public XtxImageFormat ImageFormat    { get; private set; }
		public uint           MipCount       { get; private set; }
		public uint           SliceSize      { get; private set; }
		public uint[]         MipOffsets     { get; private set; } = [];
		public uint           TextureLayout1 { get; private set; }
		public uint           TextureLayout2 { get; private set; }
		public uint           Unknown1       { get; private set; }

		public uint ArrayCount => (uint) ( DataSize / SliceSize );

		public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		{
			DataSize = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);
			Alignment = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			Width = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			Height = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			Depth = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			Target = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			ImageFormat = (XtxImageFormat) await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			MipCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			SliceSize = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			MipOffsets = new uint[17];

			for (var i = 0; i < MipOffsets.Length; i++)
				MipOffsets[i] = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

			TextureLayout1 = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			TextureLayout2 = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
			Unknown1 = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		}
	}
}