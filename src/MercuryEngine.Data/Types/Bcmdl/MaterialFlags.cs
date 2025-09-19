using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class MaterialFlags : DataStructure<MaterialFlags>
{
	public bool Flag0 { get; set; }
	public bool Flag1 { get; set; }
	public bool Flag2 { get; set; }
	public bool Flag3 { get; set; }
	public bool Flag4 { get; set; } = true;
	public bool Flag5 { get; set; }
	public bool Flag6 { get; set; } = true;
	public bool Flag7 { get; set; } = true;
	public bool Flag8 { get; set; }

	public uint Param0  { get; set; }
	public uint Param1  { get; set; } = 4;
	public uint Param2  { get; set; }
	public uint Param3  { get; set; } = 1;
	public uint Param4  { get; set; }
	public uint Param5  { get; set; }
	public uint Param6  { get; set; }
	public uint Param7  { get; set; }
	public uint Param8  { get; set; }
	public uint Param9  { get; set; }
	public uint Param10 { get; set; }
	public uint Param11 { get; set; }
	public uint Param12 { get; set; }
	public uint Param13 { get; set; }
	public uint Param14 { get; set; }
	public uint Param15 { get; set; }
	public uint Param16 { get; set; }

	protected override void Describe(DataStructureBuilder<MaterialFlags> builder)
	{
		builder.Property(m => m.Flag0);
		builder.Property(m => m.Flag1);
		builder.Property(m => m.Flag2);
		builder.Property(m => m.Flag3);
		builder.Property(m => m.Flag4);
		builder.Property(m => m.Flag5);
		builder.Property(m => m.Flag6);
		builder.Property(m => m.Flag7);
		builder.Property(m => m.Flag8);

		// 3 bytes of padding
		builder.Padding(3);

		builder.Property(m => m.Param0);
		builder.Property(m => m.Param1);
		builder.Property(m => m.Param2);
		builder.Property(m => m.Param3);
		builder.Property(m => m.Param4);
		builder.Property(m => m.Param5);
		builder.Property(m => m.Param6);
		builder.Property(m => m.Param7);
		builder.Property(m => m.Param8);
		builder.Property(m => m.Param9);
		builder.Property(m => m.Param10);
		builder.Property(m => m.Param11);
		builder.Property(m => m.Param12);
		builder.Property(m => m.Param13);
		builder.Property(m => m.Param14);
		builder.Property(m => m.Param15);
		builder.Property(m => m.Param16);

		// Trailing constant
		builder.Constant(0xFF000000FFFFFFFF, assertValueOnRead: false);
	}
}