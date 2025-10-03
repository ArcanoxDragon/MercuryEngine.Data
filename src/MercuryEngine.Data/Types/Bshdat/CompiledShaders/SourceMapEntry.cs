using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public class SourceMapEntry(SourceMapSectionHeader sourceMapHeader) : DataStructure<SourceMapEntry>
{
	public SourceMapEntryType Type { get; private set; }

	public byte[] Data { get; private set; } = [];

	#region Private Data

	private uint Size   { get; set; }
	private uint Offset { get; set; }

	private uint Unknown1 { get; set; }
	private uint Unknown2 { get; set; }
	private uint Unknown3 { get; set; }

	#endregion

	protected override void ReadCore(BinaryReader reader, ReadContext context)
	{
		base.ReadCore(reader, context);

		if (Size == 0 || Offset == 0)
		{
			Data = [];
			return;
		}

		var finalOffset = sourceMapHeader.ParentSection.DataOffset + Offset;

		using (reader.BaseStream.TemporarySeek(finalOffset))
		{
			Data = new byte[Size];
			reader.BaseStream.ReadExactly(Data.AsSpan());
		}
	}

	protected override async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		if (Size == 0 || Offset == 0)
		{
			Data = [];
			return;
		}

		var finalOffset = sourceMapHeader.ParentSection.DataOffset + Offset;

		using (reader.BaseStream.TemporarySeek(finalOffset))
		{
			Data = new byte[Size];
			await reader.BaseStream.ReadExactlyAsync(Data.AsMemory(), cancellationToken).ConfigureAwait(false);
		}
	}

	protected override void Describe(DataStructureBuilder<SourceMapEntry> builder)
	{
		builder.Property(m => m.Size);
		builder.Property(m => m.Offset);
		builder.Property(m => m.Type);
		builder.Property(m => m.Unknown1);
		builder.Property(m => m.Unknown2);
		builder.Property(m => m.Unknown3);
		builder.Padding(0xC);
	}
}