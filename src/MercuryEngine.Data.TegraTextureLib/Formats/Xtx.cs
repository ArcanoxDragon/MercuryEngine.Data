using System.Diagnostics;
using System.Text;
using MercuryEngine.Data.TegraTextureLib.Extensions;
using MercuryEngine.Data.TegraTextureLib.ImageProcessing;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public class Xtx : BaseDataFormat
{
	private const string Signature = "DFvN"; // NvFD, but stored as a little endian uint32 for some reason?

	public uint HeaderSize { get; private set; } = (uint) (
		Signature.Length +
		sizeof(uint) + // HeaderSize
		sizeof(uint) + // MajorVersion
		sizeof(uint)   // MinorVersion
	);

	public uint MajorVersion { get; private set; } = 1;
	public uint MinorVersion { get; private set; } = 1;

	public List<TegraTexture> Textures { get; } = [];

	#region Synchronous

	public override void Read(BinaryReader reader)
	{
		Textures.Clear();

		var signature = reader.ReadBytes(4);

		if (Encoding.ASCII.GetString(signature) != Signature)
			throw new IOException("Signature mismatch: not a valid XTX file");

		HeaderSize = reader.ReadUInt32();
		MajorVersion = reader.ReadUInt32();
		MinorVersion = reader.ReadUInt32();

		Debug.Assert(HeaderSize == 0x10, "Wrong header size!");

		// Read all data blocks
		var textureInfoBlocks = new List<XtxDataBlock>();
		var textureDataBlocks = new List<XtxDataBlock>();

		while (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			var block = new XtxDataBlock();

			block.Read(reader);

			if (block.BlockType == XtxBlockType.TextureInfo)
				textureInfoBlocks.Add(block);
			else if (block.BlockType == XtxBlockType.TextureData)
				textureDataBlocks.Add(block);

			// TODO: Warning for unrecognized block type?
		}

		foreach (var (i, textureInfoBlock) in textureInfoBlocks.Pairs())
		{
			if (i >= textureDataBlocks.Count)
				throw new InvalidDataException($"No texture data block found for texture {i}");

			using var textureInfoStream = new MemoryStream(textureInfoBlock.Data);
			var textureInfo = new XtxTextureInfo();
			var textureData = textureDataBlocks[i].Data;

			textureInfo.Read(textureInfoStream);
			Textures.Add(new TegraTexture(textureInfo, textureData));
		}
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Encoding.ASCII.GetBytes(Signature));
		writer.Write(HeaderSize);
		writer.Write(MajorVersion);
		writer.Write(MinorVersion);

		// Write all textures
		var nextBlockIndex = 0u;

		foreach (var texture in Textures)
		{
			// Construct texture info block, and write the texture info to its data array
			var textureInfoBlock = new XtxDataBlock {
				BlockType = XtxBlockType.TextureInfo,
				GlobalBlockIndex = nextBlockIndex++,
			};

			using (var textureInfoStream = new MemoryStream())
			{
				texture.Info.Write(textureInfoStream);
				textureInfoBlock.Data = textureInfoStream.ToArray();
			}

			// Construct texture data block with the texture's raw data
			var textureDataBlock = new XtxDataBlock {
				BlockType = XtxBlockType.TextureData,
				GlobalBlockIndex = nextBlockIndex++,
				Data = texture.Data,
				DataAlignment = texture.Info.Alignment,
			};

			// Write both blocks
			textureInfoBlock.Write(writer);
			textureDataBlock.Write(writer);
		}

		// Construct footer data block
		var footerBlock = new XtxDataBlock {
			BlockType = XtxBlockType.EndOfFile,
			GlobalBlockIndex = nextBlockIndex++,
		};

		using (var footerStream = new MemoryStream())
		{
			using var footerWriter = new BinaryWriter(footerStream);

			// TODO: Unknown values
			footerWriter.Write(0u);
			footerWriter.Write(0u);
			footerWriter.Write(1u);
			footerWriter.Write(1u);
			footerWriter.Write(0u);
			footerWriter.Write(0u);
			footerWriter.Flush();
			footerBlock.Data = footerStream.ToArray();
		}

		// Write footer
		footerBlock.Write(writer);
	}

	#endregion

	#region Asynchronous

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Textures.Clear();

		var signature = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);

		if (Encoding.ASCII.GetString(signature) != Signature)
			throw new IOException("Signature mismatch: not a valid XTX file");

		HeaderSize = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		MajorVersion = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		MinorVersion = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		reader.BaseStream.Seek(HeaderSize, SeekOrigin.Begin);

		// Read all data blocks
		var blocks = new List<XtxDataBlock>();
		var textureDatas = new List<byte[]>();

		while (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			XtxDataBlock block = new();

			await block.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			blocks.Add(block);

			if (block.BlockType == XtxBlockType.TextureData)
				textureDatas.Add(block.Data);
		}

		// Read TextureInfo block datas
		foreach (var (i, textureInfoBlock) in blocks.Where(b => b.BlockType == XtxBlockType.TextureInfo).Pairs())
		{
			if (i >= textureDatas.Count)
				throw new InvalidDataException($"No texture data block found for texture {i}");

			using var textureInfoStream = new MemoryStream(textureInfoBlock.Data);
			XtxTextureInfo textureInfo = new();
			var textureData = textureDatas[i];

			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			// (reading from a memory stream, so no async benefit and therefore not worth the overhead)
			textureInfo.Read(textureInfoStream);

			Textures.Add(new TegraTexture(textureInfo, textureData));
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Encoding.ASCII.GetBytes(Signature), cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(HeaderSize, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(MajorVersion, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(MinorVersion, cancellationToken).ConfigureAwait(false);

		// Write all textures
		var nextBlockIndex = 0u;

		foreach (var texture in Textures)
		{
			// Construct texture info block, and write the texture info to its data array
			var textureInfoBlock = new XtxDataBlock {
				BlockType = XtxBlockType.TextureInfo,
				GlobalBlockIndex = nextBlockIndex++,
			};

			using (var textureInfoStream = new MemoryStream())
			{
				// ReSharper disable once MethodHasAsyncOverloadWithCancellation
				// (writing to a memory stream, so no async benefit and therefore not worth the overhead)
				texture.Info.Write(textureInfoStream);
				textureInfoBlock.Data = textureInfoStream.ToArray();
			}

			// Construct texture data block with the texture's raw data
			var textureDataBlock = new XtxDataBlock {
				BlockType = XtxBlockType.TextureData,
				GlobalBlockIndex = nextBlockIndex++,
				Data = texture.Data,
				DataAlignment = texture.Info.Alignment,
			};

			// Write both blocks
			await textureInfoBlock.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
			await textureDataBlock.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
		}

		// Construct footer data block
		var footerBlock = new XtxDataBlock {
			BlockType = XtxBlockType.EndOfFile,
			GlobalBlockIndex = nextBlockIndex++,
		};

		using (var footerStream = new MemoryStream())
		{
			// No benefit to using async when writing to a memory stream
			await using var footerWriter = new BinaryWriter(footerStream);

			// TODO: Unknown values
			footerWriter.Write(0u);
			footerWriter.Write(0u);
			footerWriter.Write(1u);
			footerWriter.Write(1u);
			footerWriter.Write(0u);
			footerWriter.Write(0u);
			footerWriter.Flush();
			footerBlock.Data = footerStream.ToArray();
		}

		// Write footer
		await footerBlock.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	#endregion
}