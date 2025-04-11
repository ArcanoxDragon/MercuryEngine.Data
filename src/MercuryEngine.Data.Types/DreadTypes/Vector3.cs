using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector3 : DataStructure<Vector3>, ITypedDreadField
{
	public string TypeName => "base::math::CVector3D";

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	protected override void Describe(DataStructureBuilder<Vector3> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z);
}