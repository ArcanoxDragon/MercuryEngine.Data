using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using Vector3 = System.Numerics.Vector3;
using DreadVector3 = MercuryEngine.Data.Types.DreadTypes.Vector3;

namespace MercuryEngine.Data.Types.Bcmdl;

public class Transform : DataStructure<Transform>, IEquatable<Transform>
{
	public Vector3 Position
	{
		get => RawPosition;
		set => RawPosition.SetFrom(value);
	}

	public Vector3 Rotation
	{
		get => RawRotation;
		set => RawRotation.SetFrom(value);
	}

	public Vector3 Scale
	{
		get => RawScale;
		set => RawScale.SetFrom(value);
	}

	public Transform Clone()
	{
		var clone = new Transform();

		CopyTo(clone);

		return clone;
	}

	public void CopyTo(Transform other)
	{
		other.Position = Position;
		other.Rotation = Rotation;
		other.Scale = Scale;
	}

	#region Private Data

	private DreadVector3 RawPosition { get; } = new();
	private DreadVector3 RawRotation { get; } = new();
	private DreadVector3 RawScale    { get; } = new();

	// Initial matrix value should be all zeros, not Matrix4x4.Identity like the default constructor of Matrix4x4Field uses
	private Matrix4x4Field TransformMatrixField { get; } = new(default);

	#endregion

	protected override void Describe(DataStructureBuilder<Transform> builder)
	{
		builder.RawProperty(m => m.RawPosition);
		builder.RawProperty(m => m.RawRotation);
		builder.RawProperty(m => m.RawScale);
		builder.RawProperty(m => m.TransformMatrixField);
		builder.Padding(4, 0xFF);
	}

	#region Equality

	public bool Equals(Transform? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		return Position.Equals(other.Position) &&
			   Rotation.Equals(other.Rotation) &&
			   Scale.Equals(other.Scale);
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((Transform) obj);
	}

	public override int GetHashCode()
		=> HashCode.Combine(Position, Rotation, Scale);

	public static bool operator ==(Transform? left, Transform? right)
		=> Equals(left, right);

	public static bool operator !=(Transform? left, Transform? right)
		=> !Equals(left, right);

	#endregion
}