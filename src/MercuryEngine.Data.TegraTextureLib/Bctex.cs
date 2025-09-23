using System.IO.Compression;
using System.Text;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.TegraTextureLib.Extensions;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib;

public class Bctex : BaseDataFormat
{
	private const string Signature = "MTXT";

	public string? TextureName { get; private set; }
	public uint    Width       { get; private set; }
	public uint    Height      { get; private set; }
	public uint    MipCount    { get; private set; }

	public List<TegraTexture> Textures { get; } = [];

	public byte[] RawData { get; private set; } = [];

	#region Data Fields

	private uint  HeaderFlags   { get; set; }
	private ulong Unknown1      { get; set; }
	private int   Unknown2      { get; set; }
	private uint  NameOffset    { get; set; }
	private uint  Unknown3      { get; set; }
	private uint  TextureOffset { get; set; }
	private uint  Unknown4      { get; set; }
	private uint  TextureSize   { get; set; }

	private byte[] CompressedData { get; set; } = [];

	#endregion

	/// <summary>
	/// Reads a BCTEX file from the provided <paramref name="stream"/>.
	/// </summary>
	/// <param name="stream">A <see cref="Stream"/> from which the file data will be read.</param>
	/// <param name="headerOnly">
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="RawData"/> will be empty).
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
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="RawData"/> will be empty).
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
		CompressedData = compressedStream.ToArray();
		compressedStream.Seek(0, SeekOrigin.Begin);

		using var decompressedStream = new MemoryStream();

		using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			gzipStream.CopyTo(decompressedStream);

		decompressedStream.Seek(0, SeekOrigin.Begin);

		using var innerReader = new BinaryReader(decompressedStream, Encoding.UTF8);

		Unknown1 = innerReader.ReadUInt64();
		Width = innerReader.ReadUInt32();
		Height = innerReader.ReadUInt32();
		MipCount = innerReader.ReadUInt32();
		Unknown2 = innerReader.ReadInt32();
		NameOffset = innerReader.ReadUInt32();
		Unknown3 = innerReader.ReadUInt32();
		TextureOffset = innerReader.ReadUInt32();
		Unknown4 = innerReader.ReadUInt32();
		TextureSize = innerReader.ReadUInt32();

		using (decompressedStream.TemporarySeek(NameOffset - 8)) // -8 for header
			TextureName = innerReader.ReadTerminatedCString();

		decompressedStream.Seek(TextureOffset - 8, SeekOrigin.Begin); // -8 for header
		RawData = decompressedStream.ToArray();

		using var textureStream = new SlicedStream(decompressedStream, TextureOffset - 8, TextureSize);
		var xtx = new Xtx();

		xtx.Read(textureStream);

		Textures.AddRange(xtx.Textures);
	}

	/// <summary>
	/// Asynchronously reads a BCTEX file from the provided <paramref name="stream"/>.
	/// </summary>
	/// <param name="stream">A <see cref="Stream"/> from which the file data will be read.</param>
	/// <param name="headerOnly">
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="RawData"/> will be empty).
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
	/// If <see langword="true"/>, the actual texture data will not be read (<see cref="RawData"/> will be empty).
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
		CompressedData = compressedStream.ToArray();
		compressedStream.Seek(0, SeekOrigin.Begin);

		using var decompressedStream = new MemoryStream();

		await using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			await gzipStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);

		decompressedStream.Seek(0, SeekOrigin.Begin);

		using var innerReader = new AsyncBinaryReader(decompressedStream, Encoding.UTF8);

		Unknown1 = await innerReader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);
		Width = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		Height = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		MipCount = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		Unknown2 = await innerReader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
		NameOffset = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		Unknown3 = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		TextureOffset = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		Unknown4 = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		TextureSize = await innerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		using (decompressedStream.TemporarySeek(NameOffset - 8)) // -8 for header
			TextureName = await innerReader.ReadTerminatedCStringAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

		decompressedStream.Seek(TextureOffset - 8, SeekOrigin.Begin); // -8 for header
		RawData = decompressedStream.ToArray();

		await using var textureStream = new SlicedStream(decompressedStream, TextureOffset - 8, TextureSize);
		var xtx = new Xtx();

		await xtx.ReadAsync(textureStream, cancellationToken).ConfigureAwait(false);

		Textures.AddRange(xtx.Textures);
	}
}