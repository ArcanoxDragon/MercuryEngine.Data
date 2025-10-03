using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public class SourceMapSectionHeader : DataSectionHeader<SourceMapSectionHeader>
{
	internal SourceMapSectionHeader(DataSection parentSection) : base(parentSection)
	{
		SourceMap = new SourceMap(this);
	}

	public SourceMap SourceMap { get; private set; }

	#region Private Data

	private uint  Unknown1 { get; set; }
	private uint  Unknown2 { get; set; }
	private uint  Unknown3 { get; set; }
	private uint  Unknown4 { get; set; }
	private ulong Hash     { get; set; }

	#endregion

	protected override void ReadData(BinaryReader reader, ReadContext context)
		=> SourceMap.Read(reader, context);

	protected override Task ReadDataAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
		=> SourceMap.ReadAsync(reader, context, cancellationToken);

	protected override void Describe(DataStructureBuilder<SourceMapSectionHeader> builder)
	{
		builder.Property(m => m.Unknown1);
		builder.Property(m => m.Unknown2);
		builder.Property(m => m.Unknown3);
		builder.Property(m => m.Unknown4);
		builder.Property(m => m.Hash);
		builder.Padding(0x4C); // To make entire section 144 bytes
	}
}