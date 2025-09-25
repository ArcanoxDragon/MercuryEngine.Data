using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using SysVector3 = System.Numerics.Vector3;

namespace MercuryEngine.Data.Types.DreadTypes;

[DebuggerDisplay("<{X}, {Y}, {Z}>")]
public class Vector3 : DataStructure<Vector3>, ITypedDreadField, IEquatable<Vector3>, IEquatable<SysVector3>
{
	public Vector3() { }

	public Vector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vector3(SysVector3 vector)
		: this(vector.X, vector.Y, vector.Z) { }

	public string TypeName => "base::math::CVector3D";

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	public void SetFrom(SysVector3 vector)
	{
		X = vector.X;
		Y = vector.Y;
		Z = vector.Z;
	}

	protected override void Describe(DataStructureBuilder<Vector3> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z);

	#region Equality

	public bool Equals(Vector3? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		// ReSharper disable CompareOfFloatsByEqualityOperator
		return X == other.X &&
			   Y == other.Y &&
			   Z == other.Z;
		// ReSharper restore CompareOfFloatsByEqualityOperator
	}

	public bool Equals(SysVector3 other)
		=> (SysVector3) this == other;

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((Vector3) obj);
	}

	public override int GetHashCode()
		=> HashCode.Combine(X, Y, Z);

	public static bool operator ==(Vector3? left, Vector3? right)
		=> Equals(left, right);

	public static bool operator !=(Vector3? left, Vector3? right)
		=> !Equals(left, right);

	public static bool operator ==(Vector3? left, SysVector3 right)
		=> left is not null && left.Equals(right);

	public static bool operator !=(Vector3? left, SysVector3 right)
		=> left is null || !left.Equals(right);

	public static bool operator ==(SysVector3 left, Vector3? right)
		=> right is not null && right.Equals(left);

	public static bool operator !=(SysVector3 left, Vector3? right)
		=> right is null || !right.Equals(left);

	#endregion

	#region Conversions

	// Class -> Struct is implicit, no allocation required.
	// Struct -> Class is explicit, as it allocates on the heap from a struct that isn't 

	public static implicit operator SysVector3(Vector3 vector)
		=> new(vector.X, vector.Y, vector.Z);

	public static explicit operator Vector3(SysVector3 vector)
		=> new(vector);

	#endregion
}