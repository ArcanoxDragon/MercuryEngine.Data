using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class TextureParameters : DataStructure<TextureParameters>
{
	public ulong UnknownU64_0x00 { get; set; }
	public uint  UnknownU32_0x08 { get; set; }
	public float UnknownS32_0x0C { get; set; }
	public float UnknownS32_0x10 { get; set; }
	public float UnknownS32_0x14 { get; set; }
	public float UnknownS32_0x18 { get; set; }
	public float UnknownS32_0x1C { get; set; }
	public uint  UnknownU32_0x20 { get; set; }
	public uint  UnknownU32_0x24 { get; set; }
	public ulong UnknownU64_0x28 { get; set; }
	public ulong UnknownU64_0x30 { get; set; }
	public ulong UnknownU64_0x38 { get; set; }

	protected override void Describe(DataStructureBuilder<TextureParameters> builder)
	{
		builder.Property(m => m.UnknownU64_0x00);
		builder.Property(m => m.UnknownU32_0x08);
		builder.Property(m => m.UnknownS32_0x0C);
		builder.Property(m => m.UnknownS32_0x10);
		builder.Property(m => m.UnknownS32_0x14);
		builder.Property(m => m.UnknownS32_0x18);
		builder.Property(m => m.UnknownS32_0x1C);
		builder.Property(m => m.UnknownU32_0x20);
		builder.Property(m => m.UnknownU32_0x24);
		builder.Property(m => m.UnknownU64_0x28);
		builder.Property(m => m.UnknownU64_0x30);
		builder.Property(m => m.UnknownU64_0x38);
	}
}