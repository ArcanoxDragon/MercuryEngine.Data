using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector4 : DataStructure<Vector4>, IDescribeDataStructure<Vector4>
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	public static void Describe(DataStructureBuilder<Vector4> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z)
			.Property(m => m.W);
}