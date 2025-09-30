using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types;
using MercuryEngine.Data.Types.Bcmdl;
using MercuryEngine.Data.Types.Bcmdl.Wrappers;
using MercuryEngine.Data.Types.DreadTypes;
using Vector3 = System.Numerics.Vector3;

namespace MercuryEngine.Data.Formats;

public class Bcmdl : BinaryFormat<Bcmdl>
{
	[JsonIgnore]
	public override string DisplayName => "BCMDL";

	#region Public Properties

	[JsonIgnore]
	public FileVersion Version { get; } = new(1, 58, 0);

	[JsonPropertyOrder(1)]
	public IList<VertexBuffer?> VertexBuffers
	{
		get
		{
			VertexBuffersField ??= LinkedListField.Create<VertexBuffer>();
			return VertexBuffersField.Entries;
		}
	}

	[JsonPropertyOrder(2)]
	public IList<IndexBuffer?> IndexBuffers
	{
		get
		{
			IndexBuffersField ??= LinkedListField.Create<IndexBuffer>();
			return IndexBuffersField.Entries;
		}
	}

	[JsonPropertyOrder(3)]
	public IList<Mesh?> Meshes
	{
		get
		{
			MeshesField ??= LinkedListField.Create<Mesh>();
			return MeshesField.Entries;
		}
	}

	[JsonPropertyOrder(4)]
	public IList<Material?> Materials
	{
		get
		{
			MaterialsField ??= LinkedListField.Create<Material>();
			return MaterialsField.Entries;
		}
	}

	[JsonPropertyOrder(5)]
	public IList<MeshNode?> Nodes
	{
		get
		{
			NodesField ??= LinkedListField.Create<MeshNode>();
			return NodesField.Entries;
		}
	}

	[JsonPropertyOrder(6)]
	public IList<NodeId?> NodeIds
	{
		get
		{
			NodeIdsField ??= LinkedListField.Create<NodeId>();
			return NodeIdsField.Entries;
		}
	}

	[JsonPropertyOrder(7)]
	public IList<Transform?> Transforms
	{
		get
		{
			TransformsField ??= LinkedListField.Create<Transform>();
			return TransformsField.Entries;
		}
	}

	[JsonPropertyOrder(8)]
	public JointsInfo? JointsInfo { get; set; } = new();

	[JsonPropertyOrder(9)]
	public IList<SpecializationValue?> SpecializationValues
	{
		get
		{
			SpecializationValuesField ??= LinkedListField.Create<SpecializationValue>();
			return SpecializationValuesField.Entries;
		}
	}

	[JsonPropertyOrder(10)]
	public IList<UnknownMaterialParams?> UnknownMaterialParams
	{
		get
		{
			UnknownMaterialParamsField ??= LinkedListField.Create<UnknownMaterialParams>();
			return UnknownMaterialParamsField.Entries;
		}
	}

	#endregion

	#region Public Methods

	public Armature GetArmature()
		=> Armature.FromBcmdl(this);

	public NodeId GetOrCreateNodeId(string name)
	{
		var nodeId = NodeIds.FirstOrDefault(id => id?.Name == name);

		if (nodeId is null)
		{
			nodeId = new NodeId(name);
			NodeIds.Add(nodeId);
		}

		return nodeId;
	}

	public Transform GetOrCreateTransform(Vector3 position, Vector3 rotation, Vector3 scale)
	{
		var transform = Transforms.FirstOrDefault(t => t?.Position == position && t.Rotation == rotation && t.Scale == scale);

		if (transform is null)
		{
			transform = new Transform {
				Position = position,
				Rotation = rotation,
				Scale = scale,
			};
			Transforms.Add(transform);
		}

		return transform;
	}

	#endregion

	#region Private Data

	private LinkedListField<VertexBuffer>?          VertexBuffersField         { get; set; } = LinkedListField.Create<VertexBuffer>();
	private LinkedListField<IndexBuffer>?           IndexBuffersField          { get; set; } = LinkedListField.Create<IndexBuffer>();
	private LinkedListField<Mesh>?                  MeshesField                { get; set; } = LinkedListField.Create<Mesh>();
	private LinkedListField<Material>?              MaterialsField             { get; set; } = LinkedListField.Create<Material>();
	private LinkedListField<MeshNode>?              NodesField                 { get; set; } = LinkedListField.Create<MeshNode>();
	private LinkedListField<NodeId>?                NodeIdsField               { get; set; } = LinkedListField.Create<NodeId>();
	private LinkedListField<Transform>?             TransformsField            { get; set; } = LinkedListField.Create<Transform>();
	private LinkedListField<SpecializationValue>?   SpecializationValuesField  { get; set; } = LinkedListField.Create<SpecializationValue>();
	private LinkedListField<UnknownMaterialParams>? UnknownMaterialParamsField { get; set; } = LinkedListField.Create<UnknownMaterialParams>();

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		// TODO: Validate that all pointers referenced by substructures are present in the root pointer lists

		context.HeapManager.PaddingByte = 0xFF;
	}

	#endregion

	protected override void Describe(DataStructureBuilder<Bcmdl> builder)
	{
		builder.Constant("MMDL", "<magic>", terminated: false);
		builder.RawProperty(m => m.Version);
		builder.Pointer(m => m.VertexBuffersField, _ => LinkedListField.Create<VertexBuffer>());
		builder.Pointer(m => m.IndexBuffersField, _ => LinkedListField.Create<IndexBuffer>());
		builder.Pointer(m => m.MeshesField, _ => LinkedListField.Create<Mesh>());
		builder.Pointer(m => m.MaterialsField, _ => LinkedListField.Create<Material>());
		builder.Pointer(m => m.NodesField, _ => LinkedListField.Create<MeshNode>());
		builder.Pointer(m => m.NodeIdsField, _ => LinkedListField.Create<NodeId>());
		builder.Pointer(m => m.TransformsField, _ => LinkedListField.Create<Transform>());
		builder.Pointer(m => m.JointsInfo);
		builder.Pointer(m => m.SpecializationValuesField, _ => LinkedListField.Create<SpecializationValue>());
		builder.Pointer(m => m.UnknownMaterialParamsField, _ => LinkedListField.Create<UnknownMaterialParams>());
	}
}