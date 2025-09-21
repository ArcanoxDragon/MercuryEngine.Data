using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector4b : DataStructure<Vector4b>
{
	public Vector4b() { }

	public Vector4b(byte x, byte y, byte z, byte w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public byte X { get; set; }
	public byte Y { get; set; }
	public byte Z { get; set; }
	public byte W { get; set; }

	protected override void Describe(DataStructureBuilder<Vector4b> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z)
			.Property(m => m.W);
}