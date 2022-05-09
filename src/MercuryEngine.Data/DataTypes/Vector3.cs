using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class Vector3 : DataStructure<Vector3>
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	protected override void Describe(DataStructureBuilder<Vector3> builder)
		=> builder.Float(m => m.X)
				  .Float(m => m.Y)
				  .Float(m => m.Z);
}