using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector3 : DataStructure<Vector3>, IDescribeDataStructure<Vector3>
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	public static void Describe(DataStructureBuilder<Vector3> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z);
}