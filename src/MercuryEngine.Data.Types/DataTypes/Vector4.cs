using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DataTypes;

public class Vector4 : DataStructure<Vector4>
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	protected override void Describe(DataStructureBuilder<Vector4> builder)
		=> builder.Property(m => m.X)
				  .Property(m => m.Y)
				  .Property(m => m.Z)
				  .Property(m => m.W);
}