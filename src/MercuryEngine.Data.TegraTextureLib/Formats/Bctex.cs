using System.Text;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.TegraTextureLib.ImageProcessing;
using Overby.Extensions.AsyncBinaryReaderWriter;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public class Bctex : BaseDataFormat
{
	private const string Signature = "MTXT";

	private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public string?      TextureName  { get; set; }
	public TextureUsage TextureUsage { get; set; }
	public uint         Width        { get; set; }
	public uint         Height       { get; set; }
	public uint         MipCount     { get; set; }

	public List<TegraTexture> Textures { get; } = [];

	public byte[] RawData { get; private set; } = [];

	#region Data Fields

	private uint               HeaderFlags   { get; set; } = 0x00080001;
	public  MseTextureEncoding EncodingType  { get; set; }
	public  bool               IsSrgb        { get; set; }
	public  MseTextureKind     TextureKind   { get; set; }
	private int                Unknown3      { get; set; } = -1; // Padding?
	private ulong              NameOffset    { get; set; }
	private ulong              TextureOffset { get; set; }
	private uint               TextureSize   { get; set; }

	#endregion

	#region Synchronous

	/// <summary>
	/// Reads a BCTEX file from the provided <paramref name="stream"/>.
	/// </summary>
	/// <param name="stream">A <see cref="Stream"/> from which the file data will be read.</param>
	/// <param name="headerOnly">
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="Textures"/> will be empty).
	/// </param>
	/// <exception cref="IOException">Invalid data is encountered while reading the texture</exception>
	public void Read(Stream stream, bool headerOnly)
	{
		using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		Read(reader, headerOnly);
	}

	public override void Read(BinaryReader reader)
		=> Read(reader, headerOnly: false);

	/// <summary>
	/// Reads a BCTEX file using the provided <paramref name="reader"/>.
	/// </summary>
	/// <param name="reader">A <see cref="BinaryReader"/> that will be used to read the file.</param>
	/// <param name="headerOnly">
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="Textures"/> will be empty).
	/// </param>
	/// <exception cref="IOException">Invalid data is encountered while reading the texture</exception>
	public void Read(BinaryReader reader, bool headerOnly)
	{
		var signature = reader.ReadBytes(4);

		if (Encoding.ASCII.GetString(signature) != Signature)
			throw new IOException("Signature mismatch: not a valid BCTEX/MTXT file");

		HeaderFlags = reader.ReadUInt32();

		// Read the rest of the stream into memory, store a copy, and then inflate it
		using var compressedStream = new MemoryStream();

		reader.BaseStream.CopyTo(compressedStream);
		compressedStream.Seek(0, SeekOrigin.Begin);

		using var decompressedStream = new MemoryStream();

		using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			gzipStream.CopyTo(decompressedStream);

		decompressedStream.Seek(0, SeekOrigin.Begin);

		using var innerReader = new BinaryReader(decompressedStream, Encoding.UTF8);

		TextureUsage = (TextureUsage) innerReader.ReadUInt32();
		EncodingType = (MseTextureEncoding) innerReader.ReadByte();
		IsSrgb = innerReader.ReadBoolean();
		TextureKind = (MseTextureKind) innerReader.ReadUInt16();
		Width = innerReader.ReadUInt32();
		Height = innerReader.ReadUInt32();
		MipCount = innerReader.ReadUInt32();
		Unknown3 = innerReader.ReadInt32();
		NameOffset = innerReader.ReadUInt64();
		TextureOffset = innerReader.ReadUInt64();
		TextureSize = innerReader.ReadUInt32();

		using (decompressedStream.TemporarySeek((long) ( NameOffset - 8 ))) // -8 for header
			TextureName = innerReader.ReadTerminatedCString();

		if (!headerOnly)
		{
			decompressedStream.Seek((long) ( TextureOffset - 8 ), SeekOrigin.Begin); // -8 for header
			RawData = decompressedStream.ToArray();

			using var textureStream = new SlicedStream(decompressedStream, (long) ( TextureOffset - 8 ), TextureSize);
			var xtx = new Xtx();

			xtx.Read(textureStream);

			Textures.AddRange(xtx.Textures);
		}
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Encoding.ASCII.GetBytes(Signature));
		writer.Write(HeaderFlags);

		// Write the rest of the data to a temporary MemoryStream (which supports back-seeking during writing),
		// and once we're done, we will compress that stream into the main stream
		using var innerStream = new MemoryStream();
		using var innerWriter = new BinaryWriter(innerStream);

		innerWriter.Write((uint) TextureUsage);
		innerWriter.Write((byte) EncodingType);
		innerWriter.Write(IsSrgb);
		innerWriter.Write((ushort) TextureKind);
		innerWriter.Write(Width);
		innerWriter.Write(Height);
		innerWriter.Write(MipCount);
		innerWriter.Write(Unknown3);

		var textureNameOffsetLocation = innerStream.Position; // We'll need to come back to this point later

		innerWriter.Write(0ul); // Placeholder texture name pointer - we don't know it yet

		var textureOffsetLocation = innerStream.Position; // We'll need to come back to this point later

		innerWriter.Write(0ul); // Placeholder texture data pointer - we don't know it yet
		innerWriter.Write(0u);  // Placeholder for "TextureSize" - it will be written with texture data pointer

		if (Textures.Count > 0)
		{
			// Align start of XTX data with even block of 128 bytes (from the start of the file, not the start of compressed data)
			var neededPadding = MathHelper.GetNeededPaddingForAlignment((ulong) ( innerStream.Position + 8 ), 128);
			Span<byte> paddingBuffer = stackalloc byte[(int) neededPadding];

			paddingBuffer.Fill(0xFF);
			innerWriter.Write(paddingBuffer);

			// Write the texture data, and then seek back and write the pointer to it
			var textureDataStart = innerStream.Position;

			TextureOffset = (uint) ( textureDataStart + 8 ); // Offset is from start of file, not inner stream, so add +8 for header

			// For padding reasons, XTX needs to think it is starting at the beginning of the file
			using (var xtxStream = new SlicedStream(innerStream, innerStream.Position, 0))
			{
				var xtx = new Xtx();

				xtx.Textures.AddRange(Textures);
				xtx.Write(xtxStream);
			}

			var textureDataEnd = innerStream.Position;

			TextureSize = (uint) ( textureDataEnd - textureDataStart );

			// Seek back to the place where we need to write the texture data offset. We will also write TextureSize.
			using (innerStream.TemporarySeek(textureOffsetLocation))
			{
				innerWriter.Write(TextureOffset);
				innerWriter.Write(TextureSize);
			}
		}

		// Write the texture name, and then seek back and write the pointer to it
		NameOffset = (uint) ( innerStream.Position + 8 ); // Offset is from start of file, not inner stream, so add +8 for header

		innerWriter.WriteTerminatedCString(TextureName ?? string.Empty);

		using (innerStream.TemporarySeek(textureNameOffsetLocation))
			innerWriter.Write(NameOffset);

		RawData = innerStream.ToArray();

		// Compress the inner stream to a temporary stream, because GZipStream always disposes its inner stream >:(
		using var compressedStream = new MemoryStream();

		using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, CompressionLevel.BestCompression))
		{
			gzipStream.LastModified = UnixEpoch; // Dread does not use the timestamp portion of the header, so it should be all 0s
			innerStream.Seek(0, SeekOrigin.Begin);
			innerStream.CopyTo(gzipStream);
		}

		// Fix a few irregularities between Dread's GZip format and that of SharpCompress
		var compressedData = compressedStream.ToArray();

		compressedData[0x8] = 0x2; // XFL = Best Compression
		compressedData[0x9] = 0xa; // OS = 10 (Value used in base game)

		// Write the compressed data to the main stream
		writer.Write(compressedData);
	}

	#endregion

	#region Asynchronous

	/// <summary>
	/// Asynchronously reads a BCTEX file from the provided <paramref name="stream"/>.
	/// </summary>
	/// <param name="stream">A <see cref="Stream"/> from which the file data will be read.</param>
	/// <param name="headerOnly">
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="Textures"/> will be empty).
	/// </param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> instance that can be used to abort the operation.</param>
	/// <exception cref="IOException">Invalid data is encountered while reading the texture</exception>
	public async Task ReadAsync(Stream stream, bool headerOnly, CancellationToken cancellationToken = default)
	{
		using var reader = new AsyncBinaryReader(stream, Encoding.UTF8, leaveOpen: true);

		await ReadAsync(reader, headerOnly, cancellationToken).ConfigureAwait(false);
	}

	public override Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> ReadAsync(reader, headerOnly: false, cancellationToken);

	/// <summary>
	/// Asynchronously reads a BCTEX file using the provided <paramref name="reader"/>.
	/// </summary>
	/// <param name="reader">An <see cref="AsyncBinaryReader"/> that will be used to read the file.</param>
	/// <param name="headerOnly">
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="Textures"/> will be empty).
	/// </param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> instance that can be used to abort the operation.</param>
	/// <exception cref="IOException">Invalid data is encountered while reading the texture</exception>
	public async Task ReadAsync(AsyncBinaryReader reader, bool headerOnly, CancellationToken cancellationToken = default)
	{
		var signature = await reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);

		if (Encoding.ASCII.GetString(signature) != Signature)
			throw new IOException("Signature mismatch: not a valid BCTEX/MTXT file");

		HeaderFlags = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		// Read the rest of the stream into memory, store a copy, and then inflate it
		using var compressedStream = new MemoryStream();

		await reader.BaseStream.CopyToAsync(compressedStream, cancellationToken).ConfigureAwait(false);
		compressedStream.Seek(0, SeekOrigin.Begin);

		using var decompressedStream = new MemoryStream();

		await using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
		{
			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			// (reading from a memory stream, so no async benefit and therefore not worth the overhead)
			gzipStream.CopyTo(decompressedStream);
		}

		decompressedStream.Seek(0, SeekOrigin.Begin);

		// No benefit to using async when reading from a memory stream
		using var innerReader = new BinaryReader(decompressedStream, Encoding.UTF8);

		TextureUsage = (TextureUsage) innerReader.ReadUInt32();
		EncodingType = (MseTextureEncoding) innerReader.ReadByte();
		IsSrgb = innerReader.ReadBoolean();
		TextureKind = (MseTextureKind) innerReader.ReadUInt16();
		Width = innerReader.ReadUInt32();
		Height = innerReader.ReadUInt32();
		MipCount = innerReader.ReadUInt32();
		Unknown3 = innerReader.ReadInt32();
		NameOffset = innerReader.ReadUInt64();
		TextureOffset = innerReader.ReadUInt64();
		TextureSize = innerReader.ReadUInt32();

		using (decompressedStream.TemporarySeek((long) ( NameOffset - 8 ))) // -8 for header
			TextureName = innerReader.ReadTerminatedCString();

		if (!headerOnly)
		{
			decompressedStream.Seek((long) ( TextureOffset - 8 ), SeekOrigin.Begin); // -8 for header
			RawData = decompressedStream.ToArray();

			await using var textureStream = new SlicedStream(decompressedStream, (long) ( TextureOffset - 8 ), TextureSize);
			var xtx = new Xtx();

			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			// (reading from a memory stream, so no async benefit and therefore not worth the overhead)
			xtx.Read(textureStream);

			Textures.AddRange(xtx.Textures);
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(Encoding.ASCII.GetBytes(Signature), cancellationToken).ConfigureAwait(false);

		// TODO: Default value for flags? What do they do?
		await writer.WriteAsync(HeaderFlags, cancellationToken).ConfigureAwait(false);

		// Write the rest of the data to a temporary MemoryStream (which supports back-seeking during writing),
		// and once we're done, we will compress that stream into the main stream
		using var innerStream = new MemoryStream();
		await using var innerWriter = new BinaryWriter(innerStream);

		// No benefit to using async when writing to a memory stream
		innerWriter.Write((uint) TextureUsage);
		innerWriter.Write((byte) EncodingType);
		innerWriter.Write(IsSrgb);
		innerWriter.Write((ushort) TextureKind);
		innerWriter.Write(Width);
		innerWriter.Write(Height);
		innerWriter.Write(MipCount);
		innerWriter.Write(Unknown3);

		var textureNameOffsetLocation = innerStream.Position; // We'll need to come back to this point later

		innerWriter.Write(0ul); // Placeholder texture name pointer - we don't know it yet

		var textureOffsetLocation = innerStream.Position; // We'll need to come back to this point later

		innerWriter.Write(0ul); // Placeholder texture data pointer - we don't know it yet
		innerWriter.Write(0u);  // Placeholder for "TextureSize" - it will be written with texture data pointer

		if (Textures.Count > 0)
		{
			// Align start of XTX data with even block of 128 bytes (from the start of the file, not the start of compressed data)
			var neededPadding = MathHelper.GetNeededPaddingForAlignment((ulong) ( innerStream.Position + 8 ), 128);
			Span<byte> paddingBuffer = stackalloc byte[(int) neededPadding];

			paddingBuffer.Fill(0xFF);
			innerWriter.Write(paddingBuffer);

			// Write the texture data, and then seek back and write the pointer to it
			var textureDataStart = innerStream.Position;

			TextureOffset = (uint) ( textureDataStart + 8 ); // Offset is from start of file, not inner stream, so add +8 for header

			// For padding reasons, XTX needs to think it is starting at the beginning of the file
			await using (var xtxStream = new SlicedStream(innerStream, innerStream.Position, 0))
			{
				var xtx = new Xtx();

				xtx.Textures.AddRange(Textures);
				// ReSharper disable once MethodHasAsyncOverloadWithCancellation
				// (writing to a memory stream, so no async benefit and therefore not worth the overhead)
				xtx.Write(xtxStream);
			}

			var textureDataEnd = innerStream.Position;

			TextureSize = (uint) ( textureDataEnd - textureDataStart );

			// Seek back to the place where we need to write the texture data offset. We will also write TextureSize.
			using (innerStream.TemporarySeek(textureOffsetLocation))
			{
				innerWriter.Write(TextureOffset);
				innerWriter.Write(TextureSize);
			}
		}

		// Write the texture name, and then seek back and write the pointer to it
		NameOffset = (uint) ( innerStream.Position + 8 ); // Offset is from start of file, not inner stream, so add +8 for header

		innerWriter.WriteTerminatedCString(TextureName ?? string.Empty);

		using (innerStream.TemporarySeek(textureNameOffsetLocation))
			innerWriter.Write(NameOffset);

		RawData = innerStream.ToArray();

		// Compress the inner stream to a temporary stream, because GZipStream always disposes its inner stream >:(
		using var compressedStream = new MemoryStream();

		await using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, CompressionLevel.BestCompression))
		{
			gzipStream.LastModified = UnixEpoch; // Dread does not use the timestamp portion of the header, so it should be all 0s
			innerStream.Seek(0, SeekOrigin.Begin);

			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			// (writing to a memory stream, so no async benefit and therefore not worth the overhead)
			innerStream.CopyTo(gzipStream);
		}

		// Fix a few irregularities between Dread's GZip format and that of SharpCompress
		var compressedData = compressedStream.ToArray();

		compressedData[0x8] = 0x2; // XFL = Best Compression
		compressedData[0x9] = 0xa; // OS = 10 (Value used in base game)

		// Write the compressed data to the main stream
		await writer.WriteAsync(compressedData, cancellationToken).ConfigureAwait(false);
	}

	#endregion
}