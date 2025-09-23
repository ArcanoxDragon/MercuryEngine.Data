using System.Numerics;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using Vector3 = MercuryEngine.Data.Types.DreadTypes.Vector3;

namespace MercuryEngine.Data.Types.Bcmdl;

public class Submesh : DataStructure<Submesh>
{
	public Matrix4x4 TransformMatrix
	{
		get => TransformMatrixField.Value;
		set => TransformMatrixField.Value = value;
	}

	public Vector3       BoundingBoxSize { get; set; } = new();
	public IndexBuffer?  IndexBuffer     { get; set; }
	public VertexBuffer? VertexBuffer    { get; set; }
	public uint          InfoCount       { get; set; }
	public Vector3       Translation     { get; set; } = new();

	public IList<SubmeshInfo?> SubmeshInfos
	{
		get
		{
			SubmeshInfosField ??= CreateSubmeshInfosField();
			return SubmeshInfosField.Entries;
		}
	}

	#region Private Fields

	private Matrix4x4Field                TransformMatrixField { get; } = new();
	private LinkedListField<SubmeshInfo>? SubmeshInfosField    { get; set; }

	#endregion

	private static LinkedListField<SubmeshInfo> CreateSubmeshInfosField()
		=> LinkedListField.Create<SubmeshInfo>(startByteAlignment: 8);

	protected override void Describe(DataStructureBuilder<Submesh> builder)
	{
		builder.RawProperty(m => m.TransformMatrixField);
		builder.RawProperty(m => m.BoundingBoxSize);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.IndexBuffer);
		builder.Pointer(m => m.VertexBuffer);
		builder.Property(m => m.InfoCount);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.SubmeshInfosField, _ => CreateSubmeshInfosField());
		builder.RawProperty(m => m.Translation);
		builder.Padding(4, 0xFF);
	}
}