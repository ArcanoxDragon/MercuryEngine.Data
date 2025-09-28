using MercuryEngine.Data.TegraTextureLib.ImageProcessing;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.TegraTextureLib.Formats;

public sealed class XtxTextureInfo : BaseDataFormat
{
	public const int  MaxMipCount      = 17;
	public const uint DefaultAlignment = 512;

	public ulong          DataSize       { get; set; }
	public uint           Alignment      { get; set; } = DefaultAlignment;
	public uint           Width          { get; set; }
	public uint           Height         { get; set; }
	public uint           Depth          { get; set; } = 1;
	public uint           Target         { get; set; } = 1;
	public XtxImageFormat ImageFormat    { get; private set; }
	public uint           MipCount       { get; private set; }
	public uint           SliceSize      { get; private set; }
	public uint[]         MipOffsets     { get; }              = new uint[MaxMipCount];
	public uint           TextureLayout1 { get; private set; } = 4; // TODO: Unknown value
	public uint           TextureLayout2 { get; private set; } = 7; // TODO: Unknown value
	public uint           Unknown1       { get; private set; }

	public uint ArrayCount => (uint) ( DataSize / SliceSize );

	#region Synchronous

	public override void Read(BinaryReader reader)
	{
		DataSize = reader.ReadUInt64();
		Alignment = reader.ReadUInt32();
		Width = reader.ReadUInt32();
		Height = reader.ReadUInt32();
		Depth = reader.ReadUInt32();
		Target = reader.ReadUInt32();
		ImageFormat = (XtxImageFormat) reader.ReadUInt32();
		MipCount = reader.ReadUInt32();
		SliceSize = reader.ReadUInt32();

		for (var i = 0; i < MipOffsets.Length; i++)
			MipOffsets[i] = reader.ReadUInt32();

		TextureLayout1 = reader.ReadUInt32();
		TextureLayout2 = reader.ReadUInt32();
		Unknown1 = reader.ReadUInt32();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(DataSize);
		writer.Write(Alignment);
		writer.Write(Width);
		writer.Write(Height);
		writer.Write(Depth);
		writer.Write(Target);
		writer.Write((uint) ImageFormat);
		writer.Write(MipCount);
		writer.Write(SliceSize);

		foreach (var offset in MipOffsets)
			writer.Write(offset);

		writer.Write(TextureLayout1);
		writer.Write(TextureLayout2);
		writer.Write(Unknown1);
	}

	#endregion

	#region Asynchronous

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

		for (var i = 0; i < MipOffsets.Length; i++)
			MipOffsets[i] = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		TextureLayout1 = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		TextureLayout2 = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		Unknown1 = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync(DataSize, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Alignment, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Width, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Height, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Depth, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Target, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync((uint) ImageFormat, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(MipCount, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(SliceSize, cancellationToken).ConfigureAwait(false);

		foreach (var offset in MipOffsets)
			await writer.WriteAsync(offset, cancellationToken).ConfigureAwait(false);

		await writer.WriteAsync(TextureLayout1, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(TextureLayout2, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(Unknown1, cancellationToken).ConfigureAwait(false);
	}

	#endregion
}