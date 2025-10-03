using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bshdat.CompiledShaders;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat;

public class CompiledShader : DataStructure<CompiledShader>
{
	public uint FileSize { get; private set; }

	public DataSection[] Sections { get; private set; } = [];

	#region Private Data

	private uint  BaseDataOffset { get; set; }
	private uint  SectionCount   { get; set; }
	private ulong Flags          { get; set; }
	private uint  Unknown0       { get; set; }
	private uint  Unknown1       { get; set; }
	private uint  Unknown2       { get; set; }
	private uint  Unknown3       { get; set; }
	private uint  Unknown4       { get; set; }

	#endregion

	protected override void BeforeWrite(WriteContext context)
	{
		throw new NotSupportedException("Writing is not supported at this time");
	}

	protected override void ReadCore(BinaryReader reader, ReadContext context)
	{
		base.ReadCore(reader, context);

		Sections = new DataSection[SectionCount];

		for (var i = 0; i < SectionCount; i++)
		{
			Sections[i] = new DataSection();
			Sections[i].Read(reader, context);
		}
	}

	protected override async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		Sections = new DataSection[SectionCount];

		for (var i = 0; i < SectionCount; i++)
		{
			Sections[i] = new DataSection();
			await Sections[i].ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		}
	}

	protected override void Describe(DataStructureBuilder<CompiledShader> builder)
	{
		builder.Constant(0x19866891, "<magic>");
		builder.Padding(4);
		builder.Property(m => m.Flags);
		builder.Property(m => m.Unknown0);
		builder.Property(m => m.Unknown1);
		builder.Property(m => m.Unknown2);
		builder.Property(m => m.Unknown3);
		builder.Property(m => m.Unknown4);
		builder.Padding(0x20);
		builder.Property(m => m.FileSize);
		builder.Property(m => m.BaseDataOffset);
		builder.Property(m => m.SectionCount);
		builder.Padding(0x40);
	}
}