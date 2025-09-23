using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bcmdl;

public class JointMap(Submesh parent) : IBinaryField
{
	private uint[] entries = [];

	public uint[] Entries
	{
		get
		{
			ResizeArrayIfNeeded();
			return this.entries;
		}
	}

	public uint Size => parent.JointMapEntryCount * sizeof(uint);

	public void Read(BinaryReader reader, ReadContext context)
	{
		ResizeArrayIfNeeded();

		for (var i = 0; i < this.entries.Length; i++)
			this.entries[i] = reader.ReadUInt32();
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		ResizeArrayIfNeeded();

		foreach (var entry in this.entries)
			writer.Write(entry);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		ResizeArrayIfNeeded();

		for (var i = 0; i < this.entries.Length; i++)
			this.entries[i] = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		ResizeArrayIfNeeded();

		foreach (var entry in this.entries)
			await writer.WriteAsync(entry, cancellationToken).ConfigureAwait(false);
	}

	private void ResizeArrayIfNeeded()
	{
		if (this.entries.Length == parent.JointMapEntryCount)
			return;

		var newArray = new uint[parent.JointMapEntryCount];

		Array.Copy(this.entries, newArray, Math.Min(this.entries.Length, newArray.Length));
		this.entries = newArray;
	}
}