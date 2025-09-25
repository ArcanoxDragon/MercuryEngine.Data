using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using SysVector4 = System.Numerics.Vector4;

namespace MercuryEngine.Data.Types.DreadTypes;

[DebuggerDisplay("<{X}, {Y}, {Z}, {W}>")]
public class Vector4 : DataStructure<Vector4>, ITypedDreadField, IEquatable<Vector4>, IEquatable<SysVector4>
{
	public Vector4() { }

	public Vector4(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Vector4(SysVector4 vector)
		: this(vector.X, vector.Y, vector.Z, vector.W) { }

	public string TypeName => "base::math::CVector4D";

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	public void SetFrom(SysVector4 vector)
	{
		X = vector.X;
		Y = vector.Y;
		Z = vector.Z;
		W = vector.W;
	}

	protected override void Describe(DataStructureBuilder<Vector4> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y)
			.Property(m => m.Z)
			.Property(m => m.W);

	#region Equality

	public bool Equals(Vector4? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		// ReSharper disable CompareOfFloatsByEqualityOperator
		return X == other.X &&
			   Y == other.Y &&
			   Z == other.Z &&
			   W == other.W;
		// ReSharper restore CompareOfFloatsByEqualityOperator
	}

	public bool Equals(SysVector4 other)
		=> (SysVector4) this == other;

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((Vector4) obj);
	}

	public override int GetHashCode()
		=> HashCode.Combine(X, Y, Z, W);

	public static bool operator ==(Vector4? left, Vector4? right)
		=> Equals(left, right);

	public static bool operator !=(Vector4? left, Vector4? right)
		=> !Equals(left, right);

	public static bool operator ==(Vector4? left, SysVector4 right)
		=> left is not null && left.Equals(right);

	public static bool operator !=(Vector4? left, SysVector4 right)
		=> left is null || !left.Equals(right);

	public static bool operator ==(SysVector4 left, Vector4? right)
		=> right is not null && right.Equals(left);

	public static bool operator !=(SysVector4 left, Vector4? right)
		=> right is null || !right.Equals(left);

	#endregion

	#region Conversions

	// Class -> Struct is implicit, no allocation required.
	// Struct -> Class is explicit, as it allocates on the heap from a struct that isn't 

	public static implicit operator SysVector4(Vector4 vector)
		=> new(vector.X, vector.Y, vector.Z, vector.W);

	public static explicit operator Vector4(SysVector4 vector)
		=> new(vector);

	#endregion
}