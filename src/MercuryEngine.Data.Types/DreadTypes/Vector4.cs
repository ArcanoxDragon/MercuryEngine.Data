using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using SysVector4 = System.Numerics.Vector4;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector4 : DataStructure<Vector4>, ITypedDreadField
{
	public Vector4() { }

	public Vector4(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public string TypeName => "base::math::CVector4D";

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	protected override void Describe(DataStructureBuilder<Vector4> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z)
			.Property(m => m.W);

	#region Conversions

	// Class -> Struct is implicit, no allocation required.
	// Struct -> Class is explicit, as it allocates on the heap from a struct that isn't 

	public static implicit operator SysVector4(Vector4 vector)
		=> new(vector.X, vector.Y, vector.Z, vector.W);

	public static explicit operator Vector4(SysVector4 vector)
		=> new(vector.X, vector.Y, vector.Z, vector.W);

	#endregion
}