using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bsmat;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public class BytecodeSectionHeader : DataSectionHeader<BytecodeSectionHeader>
{
	internal BytecodeSectionHeader(DataSection parentSection) : base(parentSection) { }

	public ShaderType ShaderType { get; set; }

	#region Private Data

	private uint UnkOffset1 { get; set; }
	private uint UnkOffset2 { get; set; }
	private uint UnkSize1   { get; set; }
	private uint UnkSize2   { get; set; }

	private uint Unknown3 { get; set; }
	private uint Unknown4 { get; set; }
	private uint Unknown5 { get; set; }
	private uint Unknown6 { get; set; }

	private uint UnkOffset7 { get; set; }
	private uint UnkSize7   { get; set; }

	#endregion

	protected override void Describe(DataStructureBuilder<BytecodeSectionHeader> builder)
	{
		builder.Property(m => m.ShaderType);
		builder.Property(m => m.UnkOffset1);
		builder.Property(m => m.UnkOffset2);
		builder.Property(m => m.UnkSize1);
		builder.Property(m => m.UnkSize2);
		builder.Property(m => m.Unknown3);
		builder.Property(m => m.Unknown4);
		builder.Property(m => m.Unknown5);
		builder.Property(m => m.Unknown6);
		builder.Property(m => m.UnkOffset7);
		builder.Property(m => m.UnkSize7);
		builder.Padding(0x38); // To make total section size = 144 bytes
	}
}