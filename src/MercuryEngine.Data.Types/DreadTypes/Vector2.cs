using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using SysVector2 = System.Numerics.Vector2;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector2 : DataStructure<Vector2>, ITypedDreadField
{
	public Vector2() { }

	public Vector2(float x, float y)
	{
		X = x;
		Y = y;
	}

	public string TypeName => "base::math::CVector2D";

	public float X { get; set; }
	public float Y { get; set; }

	protected override void Describe(DataStructureBuilder<Vector2> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y);

	#region Conversions

	// Class -> Struct is implicit, no allocation required.
	// Struct -> Class is explicit, as it allocates on the heap from a struct that isn't 

	public static implicit operator SysVector2(Vector2 vector)
		=> new(vector.X, vector.Y);

	public static explicit operator Vector2(SysVector2 vector)
		=> new(vector.X, vector.Y);

	#endregion
}