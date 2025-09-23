using System.Numerics;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using Vector3 = MercuryEngine.Data.Types.DreadTypes.Vector3;

namespace MercuryEngine.Data.Types.Bcmdl;

public class Mesh : DataStructure<Mesh>
{
	public Matrix4x4 TransformMatrix
	{
		get => TransformMatrixField.Value;
		set => TransformMatrixField.Value = value;
	}

	public Vector3       BoundingBoxSize { get; set; } = new();
	public IndexBuffer?  IndexBuffer     { get; set; }
	public VertexBuffer? VertexBuffer    { get; set; }
	public Vector3       Translation     { get; set; } = new();

	public IList<Submesh?> Submeshes
	{
		get
		{
			SubmeshesField ??= CreateSubmeshesField();
			return SubmeshesField.Entries;
		}
	}

	#region Private Fields

	private Matrix4x4Field            TransformMatrixField { get; } = new();
	private uint                      SubmeshCount         { get; set; }
	private LinkedListField<Submesh>? SubmeshesField       { get; set; }

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		SubmeshCount = (uint) ( SubmeshesField?.Entries.Count ?? 0 );
	}

	#endregion

	private static LinkedListField<Submesh> CreateSubmeshesField()
		=> LinkedListField.Create<Submesh>(startByteAlignment: 8);

	protected override void Describe(DataStructureBuilder<Mesh> builder)
	{
		builder.RawProperty(m => m.TransformMatrixField);
		builder.RawProperty(m => m.BoundingBoxSize);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.IndexBuffer);
		builder.Pointer(m => m.VertexBuffer);
		builder.Property(m => m.SubmeshCount);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.SubmeshesField, _ => CreateSubmeshesField());
		builder.RawProperty(m => m.Translation);
		builder.Padding(4, 0xFF);
	}
}