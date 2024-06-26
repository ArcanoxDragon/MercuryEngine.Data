using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector2 : DataStructure<Vector2>, IDescribeDataStructure<Vector2>
{
	public float X { get; set; }
	public float Y { get; set; }

	public static void Describe(DataStructureBuilder<Vector2> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y);
}