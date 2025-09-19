using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types;
using MercuryEngine.Data.Types.Bcmdl;

namespace MercuryEngine.Data.Formats;

public class Bcmdl : BinaryFormat<Bcmdl>
{
	public override string DisplayName => "BCMDL";

	#region Public Properties

	public IList<VertexBuffer?> VertexBuffers
	{
		get
		{
			VertexBuffersField ??= LinkedListField.Create<VertexBuffer>();
			return VertexBuffersField.Entries;
		}
	}

	public IList<IndexBuffer?> IndexBuffers
	{
		get
		{
			IndexBuffersField ??= LinkedListField.Create<IndexBuffer>();
			return IndexBuffersField.Entries;
		}
	}

	public IList<Submesh?> Submeshes
	{
		get
		{
			SubmeshesField ??= LinkedListField.Create<Submesh>();
			return SubmeshesField.Entries;
		}
	}

	public IList<Material?> Materials
	{
		get
		{
			MaterialsField ??= LinkedListField.Create<Material>();
			return MaterialsField.Entries;
		}
	}

	public IList<Mesh?> Meshes
	{
		get
		{
			MeshesField ??= LinkedListField.Create<Mesh>();
			return MeshesField.Entries;
		}
	}

	public IList<MeshId?> MeshIds
	{
		get
		{
			MeshIdsField ??= LinkedListField.Create<MeshId>();
			return MeshIdsField.Entries;
		}
	}

	public IList<Transform?> Transforms
	{
		get
		{
			TransformsField ??= LinkedListField.Create<Transform>();
			return TransformsField.Entries;
		}
	}

	public JointsInfo? JointsInfo { get; set; }

	public IList<SpecializationValue?> SpecializationValues
	{
		get
		{
			SpecializationValuesField ??= LinkedListField.Create<SpecializationValue>();
			return SpecializationValuesField.Entries;
		}
	}

	#endregion

	#region Private Data

	private LinkedListField<VertexBuffer>?        VertexBuffersField        { get; set; }
	private LinkedListField<IndexBuffer>?         IndexBuffersField         { get; set; }
	private LinkedListField<Submesh>?             SubmeshesField            { get; set; }
	private LinkedListField<Material>?            MaterialsField            { get; set; }
	private LinkedListField<Mesh>?                MeshesField               { get; set; }
	private LinkedListField<MeshId>?              MeshIdsField              { get; set; }
	private LinkedListField<Transform>?           TransformsField           { get; set; }
	private LinkedListField<SpecializationValue>? SpecializationValuesField { get; set; }

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		context.HeapManager.PaddingByte = 0xFF;
	}

	#endregion

	protected override void Describe(DataStructureBuilder<Bcmdl> builder)
	{
		builder.Constant("MMDL", "<magic>", terminated: false);
		builder.Constant(0x003A0001, "<version>");
		builder.Pointer(m => m.VertexBuffersField, _ => LinkedListField.Create<VertexBuffer>());
		builder.Pointer(m => m.IndexBuffersField, _ => LinkedListField.Create<IndexBuffer>());
		builder.Pointer(m => m.SubmeshesField, _ => LinkedListField.Create<Submesh>());
		builder.Pointer(m => m.MaterialsField, _ => LinkedListField.Create<Material>());
		builder.Pointer(m => m.MeshesField, _ => LinkedListField.Create<Mesh>());
		builder.Pointer(m => m.MeshIdsField, _ => LinkedListField.Create<MeshId>());
		builder.Pointer(m => m.TransformsField, _ => LinkedListField.Create<Transform>());
		builder.Pointer(m => m.JointsInfo);
		builder.Pointer(m => m.SpecializationValuesField, _ => LinkedListField.Create<SpecializationValue>());

		// TODO: "Struct9" pointer!
		builder.Constant((ulong) 0);
	}
}