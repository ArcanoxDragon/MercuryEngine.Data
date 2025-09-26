using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bcmdl;

public class VertexInfoList : IBinaryField
{
	public List<VertexInfoDescription> Entries { get; } = [];

	public uint GetSize(uint startPosition)
	{
		var totalSize = (uint) (
			sizeof(uint) + // Count
			sizeof(uint)   // Padding
		);
		var currentPosition = startPosition + totalSize;

		foreach (var entry in Entries)
		{
			var entrySize = entry.GetSize(currentPosition);

			totalSize += entrySize;
			currentPosition += entrySize;
		}

		return totalSize;
	}

	public void Read(BinaryReader reader, ReadContext context)
	{
		Entries.Clear();

		var entryCount = reader.ReadUInt32();

		// There are four bytes of padding between the entry count and the first entry (-1 if read as a signed int32)
		Debug.Assert(reader.ReadInt32() == -1);

		for (var i = 0; i < entryCount; i++)
		{
			var entry = new VertexInfoDescription();

			entry.Read(reader, context);
			Entries.Add(entry);
		}
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		writer.Write((uint) Entries.Count);

		// There are four bytes of padding between the entry count and the first entry (-1 if read as a signed int32)
		writer.Write(-1);

		foreach (var entry in Entries)
			entry.Write(writer, context);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		Entries.Clear();

		var entryCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		// There are four bytes of padding between the entry count and the first entry (-1 if read as a signed int32)
		Debug.Assert(await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false) == -1);

		for (var i = 0; i < entryCount; i++)
		{
			var entry = new VertexInfoDescription();

			await entry.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
			Entries.Add(entry);
		}
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync((uint) Entries.Count, cancellationToken).ConfigureAwait(false);

		// There are four bytes of padding between the entry count and the first entry (-1 if read as a signed int32)
		await writer.WriteAsync(-1, cancellationToken).ConfigureAwait(false);

		foreach (var entry in Entries)
			await entry.WriteAsync(writer, context, cancellationToken).ConfigureAwait(false);
	}
}