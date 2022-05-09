using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class Vector4 : DataStructure<Vector4>
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	protected override void Describe(DataStructureBuilder<Vector4> builder)
		=> builder.Float(m => m.X)
				  .Float(m => m.Y)
				  .Float(m => m.Z)
				  .Float(m => m.W);
}