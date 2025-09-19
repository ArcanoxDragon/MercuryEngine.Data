using System.Numerics;
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

	public Matrix4x4 TransformMatrix
	{
		get => TransformMatrixField.Value;
		set => TransformMatrixField.Value = value;
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