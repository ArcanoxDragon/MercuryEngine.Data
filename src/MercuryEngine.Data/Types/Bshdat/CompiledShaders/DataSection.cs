using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public sealed class DataSection : DataStructure<DataSection>
{
	public DataSection()
	{
		GenericHeader = new GenericSectionHeader(this);
		BytecodeHeader = new BytecodeSectionHeader(this);
		AssemblyHeader = new AssemblySectionHeader(this);
		ReflectionHeader = new ReflectionSectionHeader(this);
		SourceMapHeader = new SourceMapSectionHeader(this);

		SectionHeaderField = SwitchField<IDataSectionHeader>.FromProperty(this, m => m.Type, builder => {
			builder.AddCase(SectionType.Bytecode, BytecodeHeader);
			builder.AddCase(SectionType.Assembly, AssemblyHeader);
			builder.AddCase(SectionType.Reflection, ReflectionHeader);
			builder.AddCase(SectionType.SourceMap, SourceMapHeader);
			builder.AddFallback(GenericHeader);
		});
	}

	public uint        DataSize   { get; private set; }
	public uint        DataOffset { get; private set; }
	public SectionType Type       { get; private set; }

	public IDataSectionHeader SectionHeader => SectionHeaderField.EffectiveField;

	#region Private Data

	private GenericSectionHeader    GenericHeader    { get; }
	private BytecodeSectionHeader   BytecodeHeader   { get; }
	private AssemblySectionHeader   AssemblyHeader   { get; }
	private ReflectionSectionHeader ReflectionHeader { get; }
	private SourceMapSectionHeader  SourceMapHeader  { get; }

	private SwitchField<IDataSectionHeader> SectionHeaderField { get; }

	#endregion

	protected override void Describe(DataStructureBuilder<DataSection> builder)
	{
		builder.Property(m => m.DataSize);
		builder.Property(m => m.DataOffset);
		builder.Property(m => m.Type);
		builder.Padding(0x20);
		builder.RawProperty(m => m.SectionHeaderField);
	}

	public enum SectionType
	{
		Bytecode,
		Assembly,
		Unknown,
		Reflection,
		SourceMap,
	}
}