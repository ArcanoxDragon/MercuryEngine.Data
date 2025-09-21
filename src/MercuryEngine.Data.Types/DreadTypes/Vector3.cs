using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using SysVector3 = System.Numerics.Vector3;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector3 : DataStructure<Vector3>, ITypedDreadField
{
	public Vector3() { }

	public Vector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public string TypeName => "base::math::CVector3D";

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	protected override void Describe(DataStructureBuilder<Vector3> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z);

	#region Conversions

	// Class -> Struct is implicit, no allocation required.
	// Struct -> Class is explicit, as it allocates on the heap from a struct that isn't 

	public static implicit operator SysVector3(Vector3 vector)
		=> new(vector.X, vector.Y, vector.Z);

	public static explicit operator Vector3(SysVector3 vector)
		=> new(vector.X, vector.Y, vector.Z);

	#endregion
}