using System.Diagnostics;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;
using SysVector2 = System.Numerics.Vector2;

namespace MercuryEngine.Data.Types.DreadTypes;

[DebuggerDisplay("<{X}, {Y}>")]
public class Vector2 : DataStructure<Vector2>, ITypedDreadField, IEquatable<Vector2>, IEquatable<SysVector2>
{
	public Vector2() { }

	public Vector2(float x, float y)
	{
		X = x;
		Y = y;
	}

	public Vector2(SysVector2 vector)
		: this(vector.X, vector.Y) { }

	public string TypeName => "base::math::CVector2D";

	public float X { get; set; }
	public float Y { get; set; }

	public void SetFrom(SysVector2 vector)
	{
		X = vector.X;
		Y = vector.Y;
	}

	protected override void Describe(DataStructureBuilder<Vector2> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y);

	#region Equality

	public bool Equals(Vector2? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		// ReSharper disable CompareOfFloatsByEqualityOperator
		return X == other.X &&
			   Y == other.Y;
		// ReSharper restore CompareOfFloatsByEqualityOperator
	}

	public bool Equals(SysVector2 other)
		=> (SysVector2) this == other;

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((Vector2) obj);
	}

	public override int GetHashCode()
		=> HashCode.Combine(X, Y);

	public static bool operator ==(Vector2? left, Vector2? right)
		=> Equals(left, right);

	public static bool operator !=(Vector2? left, Vector2? right)
		=> !Equals(left, right);

	public static bool operator ==(Vector2? left, SysVector2 right)
		=> left is not null && left.Equals(right);

	public static bool operator !=(Vector2? left, SysVector2 right)
		=> left is null || !left.Equals(right);

	public static bool operator ==(SysVector2 left, Vector2? right)
		=> right is not null && right.Equals(left);

	public static bool operator !=(SysVector2 left, Vector2? right)
		=> right is null || !right.Equals(left);

	#endregion

	#region Conversions

	// Class -> Struct is implicit, no allocation required.
	// Struct -> Class is explicit, as it allocates on the heap from a struct that isn't 

	public static implicit operator SysVector2(Vector2 vector)
		=> new(vector.X, vector.Y);

	public static explicit operator Vector2(SysVector2 vector)
		=> new(vector);

	#endregion
}