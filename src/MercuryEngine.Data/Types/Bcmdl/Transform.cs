using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using Vector3 = MercuryEngine.Data.Types.DreadTypes.Vector3;

namespace MercuryEngine.Data.Types.Bcmdl;

public class Transform : DataStructure<Transform>
{
	public Vector3 Position { get; set; } = new();
	public Vector3 Rotation { get; set; } = new();
	public Vector3 Scale    { get; set; } = new();

	public Transform Clone()
	{
		var clone = new Transform();

		CopyTo(clone);

		return clone;
	}

	public void CopyTo(Transform other)
	{
		other.Position.X = Position.X;
		other.Position.Y = Position.Y;
		other.Position.Z = Position.Z;

		other.Rotation.X = Rotation.X;
		other.Rotation.Y = Rotation.Y;
		other.Rotation.Z = Rotation.Z;

		other.Scale.X = Scale.X;
		other.Scale.Y = Scale.Y;
		other.Scale.Z = Scale.Z;
	}

	#region Private Data

	private Matrix4x4Field TransformMatrixField { get; } = new();

	#endregion

	protected override void Describe(DataStructureBuilder<Transform> builder)
	{
		builder.RawProperty(m => m.Position);
		builder.RawProperty(m => m.Rotation);
		builder.RawProperty(m => m.Scale);
		builder.RawProperty(m => m.TransformMatrixField);
		builder.Padding(4, 0xFF);
	}
}