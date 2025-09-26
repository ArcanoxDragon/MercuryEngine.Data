using System.Numerics;
using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Bcmdl;
using MercuryEngine.Data.Types.Bcmdl.Wrappers;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using Mesh = MercuryEngine.Data.Types.Bcmdl.Mesh;
using MeshPrimitive = MercuryEngine.Data.Types.Bcmdl.MeshPrimitive;
using SGMesh = SharpGLTF.Schema2.Mesh;
using SGMeshPrimitive = SharpGLTF.Schema2.MeshPrimitive;
using SysVector2 = System.Numerics.Vector2;
using SysVector3 = System.Numerics.Vector3;
using SysVector4 = System.Numerics.Vector4;
using Vector3 = MercuryEngine.Data.Types.DreadTypes.Vector3;

namespace MercuryEngine.Data.Converters.Bcmdl;

public class GltfImporter
{
	private GltfImportResult?       CurrentResult            { get; set; }
	private Dictionary<int, uint>   JointIndexTranslationMap { get; } = [];
	private Dictionary<uint, Joint> JointsByIndex            { get; } = [];

	public GltfImportResult ImportGltf(string sourceFilePath)
	{
		try
		{
			var targetModel = new Formats.Bcmdl();

			CurrentResult = new GltfImportResult(targetModel);
			JointIndexTranslationMap.Clear();
			JointsByIndex.Clear();

			var readSettings = new ReadSettings {
				ImageDecoder = DecodeTextureImage,
				Validation = ValidationMode.Skip,
			};
			var modelRoot = ModelRoot.Load(sourceFilePath, readSettings);

			// Create Joints from the scene's skin joints
			foreach (var node in modelRoot.DefaultScene.VisualChildren)
				ImportJointsFromNodeHierarchy(node);

			// Create MeshNodes from the scene's visual children
			foreach (var node in modelRoot.DefaultScene.VisualChildren)
				ImportMeshesFromNodeHierarchy(node);

			// Fill in JointFlags with all falses
			/*if (targetModel.JointsInfo is { } jointsInfo)
				Array.Fill(jointsInfo.JointFlags, false);*/

			return CurrentResult;
		}
		finally
		{
			CurrentResult = null;
			JointIndexTranslationMap.Clear();
			JointsByIndex.Clear();
		}
	}

	private bool DecodeTextureImage(Image image)
	{
		// TODO: Decode to BCTEX
		return false;
	}

	#region Joints

	private void ImportJointsFromNodeHierarchy(Node node)
	{
		if (IsJointOrEmptyNode(node))
		{
			// This starts processing an entire hierarchy, so we don't recurse afterwards
			ImportJointsFromNodeHierarchyCore(node);
		}
		else
		{
			// Recursively try to find a joint root in the children
			foreach (var child in node.VisualChildren)
				ImportJointsFromNodeHierarchy(child);
		}
	}

	private void ImportJointsFromNodeHierarchyCore(Node node, Node? parent = null)
	{
		// We treat empty nodes as joints too, because there's no way in glTF to designate
		// nodes as bones that are not actually skinned to a mesh. Dread relies on "marker"
		// joints that are not in any way involved in skinning.
		if (!IsJointOrEmptyNode(node))
			return;

		MathHelper.DecomposeWithEulerRotation(node.LocalMatrix, out var translation, out var rotation, out var scale);

		// Fix translation scale
		translation = ScalePosition(translation);

		var joint = new Joint {
			Name = node.Name,
			ParentName = parent?.Name,
			Transform = CurrentResult!.Model.GetOrCreateTransform(translation, rotation, scale),
		};

		CurrentResult.Model.JointsInfo ??= new JointsInfo();

		var thisJointIndex = (uint) CurrentResult.Model.JointsInfo.Joints.Count;

		// Map the "logical index" of the joint node to its index in the BCMDL
		JointIndexTranslationMap[node.LogicalIndex] = thisJointIndex;
		JointsByIndex[thisJointIndex] = joint;
		CurrentResult.Model.JointsInfo.Joints.Add(joint);

		// Recurse children
		foreach (var child in node.VisualChildren)
			ImportJointsFromNodeHierarchyCore(child, node);
	}

	#endregion

	#region Meshes

	private void ImportMeshesFromNodeHierarchy(Node node)
	{
		if (node.Mesh is { } mesh)
			ImportMeshNode(node, mesh);

		foreach (var child in node.VisualChildren)
			ImportMeshesFromNodeHierarchy(child);
	}

	private void ImportMeshNode(Node node, SGMesh mesh)
	{
		var meshNodeName = node.Name ?? mesh.Name;
		var meshNode = new MeshNode {
			Mesh = new Mesh {
				Translation = new Vector3(ScalePosition(node.LocalTransform.Translation)),
			},
		};

		// Fix the scale of the transform matrix's translation
		var meshMatrix = new Matrix4x4 {
			Translation = GltfImporter.ScalePosition(node.WorldMatrix.Translation),
		};

		meshNode.Mesh.TransformMatrix = meshMatrix;
		meshNode.Mesh.Translation.SetFrom(ScalePosition(node.LocalTransform.Translation));

		if (meshNodeName != null)
			meshNode.Id = CurrentResult!.Model.GetOrCreateNodeId(meshNodeName);

		var indices = new List<ushort>();
		var vertices = new List<VertexData>();

		foreach (var primitive in mesh.Primitives)
		{
			if (primitive.DrawPrimitiveType != PrimitiveType.TRIANGLES)
				// TODO: It's possible the BCMDL format encodes the primitive geometry type in an unidentified field
				throw new NotSupportedException("Only triangle primitives are supported");

			var bcmdlPrimitive = ParsePrimitiveData(primitive, indices, vertices);

			if (node.Skin is { } skin)
				LinkPrimitiveJoints(bcmdlPrimitive, skin);

			meshNode.Mesh.Primitives.Add(bcmdlPrimitive);
		}

		// Calculate bounding box size
		if (vertices.Count > 0)
		{
			var minPosition = SysVector3.PositiveInfinity;
			var maxPosition = SysVector3.NegativeInfinity;

			foreach (var vertex in vertices)
			{
				if (!vertex.Position.HasValue)
					continue;

				minPosition = SysVector3.Min(minPosition, vertex.Position.Value);
				maxPosition = SysVector3.Max(maxPosition, vertex.Position.Value);
			}

			meshNode.Mesh.BoundingBoxSize = new Vector3(SysVector3.Abs(maxPosition - minPosition));
		}

		// Construct vertex and index buffer
		var indexBuffer = new IndexBuffer { IsCompressed = true };
		var vertexBuffer = new VertexBuffer { IsCompressed = true };

		indexBuffer.ReplaceIndices(CollectionsMarshal.AsSpan(indices));
		vertexBuffer.ReplaceVertices(CollectionsMarshal.AsSpan(vertices));

		// Install buffers to both the mesh and the BCMDL
		meshNode.Mesh.IndexBuffer = indexBuffer;
		meshNode.Mesh.VertexBuffer = vertexBuffer;
		CurrentResult!.Model.IndexBuffers.Add(indexBuffer);
		CurrentResult!.Model.VertexBuffers.Add(vertexBuffer);

		// Add the MeshNode and Mesh to the BCMDL
		CurrentResult!.Model.Meshes.Add(meshNode.Mesh);
		CurrentResult!.Model.Nodes.Add(meshNode);

		// TODO: Materials!
	}

	private static MeshPrimitive ParsePrimitiveData(SGMeshPrimitive primitive, List<ushort> indices, List<VertexData> vertices)
	{
		// Import the primitive's index buffer data
		var indexStart = indices.Count;
		var firstVertexIndex = vertices.Count;
		var primitiveIndices = primitive.GetIndices();

		foreach (var index in primitiveIndices)
		{
			// Index values in the glTF primitive are local to this primitive (i.e. start at 0 for each primitive),
			// but the index and vertex data for all primitives are combined into a single index buffer and single
			// vertex buffer for the BCMDL model. We have to translate the glTF indices to proper BCMDL indices.
			indices.Add((ushort) ( index + firstVertexIndex ));
		}

		// Import the primitive's vertex buffer data
		VertexData[]? vertexData = null;

		foreach (var (attributeKey, accessor) in primitive.VertexAccessors)
		{
			if (vertexData is null)
			{
				vertexData = new VertexData[accessor.Count];

				for (var i = 0; i < vertexData.Length; i++)
					vertexData[i] = new VertexData();
			}

			switch (attributeKey)
			{
				case GltfAttributeKeys.Position:
					PopulatePositions(accessor.AsVector3Array());
					break;
				case GltfAttributeKeys.Normal:
					PopulateNormals(accessor.AsVector3Array());
					break;
				case GltfAttributeKeys.Tangent:
					PopulateTangents(accessor.AsVector4Array());
					break;
				case GltfAttributeKeys.UV1:
					PopulateUV1(accessor.AsVector2Array());
					break;
				case GltfAttributeKeys.UV2:
					PopulateUV2(accessor.AsVector2Array());
					break;
				case GltfAttributeKeys.UV3:
					PopulateUV3(accessor.AsVector2Array());
					break;
				case GltfAttributeKeys.Color:
					PopulateColors(accessor.AsVector4Array());
					break;
				case GltfAttributeKeys.JointIndex:
					PopulateJointIndices(accessor.AsVector4Array());
					break;
				case GltfAttributeKeys.JointWeight:
					PopulateJointWeights(accessor.AsVector4Array());
					break;
				default:
					// TODO: Provide warnings to consumer
					continue;
			}
		}

		if (vertexData != null)
			vertices.AddRange(vertexData);

		return new MeshPrimitive {
			IndexOffset = (uint) indexStart,
			IndexCount = (uint) primitiveIndices.Count,
			// TODO: Not sure if this is universal for glTFs or not...might need to analyze joint bind matrices?
			SkinningType = SkinningType.PerJointTransform,
		};

		// TODO: JointMap

		void PopulatePositions(IAccessorArray<SysVector3> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].Position = ScalePosition(accessorArray[i]);
		}

		void PopulateNormals(IAccessorArray<SysVector3> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].Normal = accessorArray[i];
		}

		void PopulateTangents(IAccessorArray<SysVector4> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].Tangent = accessorArray[i];
		}

		void PopulateUV1(IAccessorArray<SysVector2> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].UV1 = accessorArray[i];
		}

		void PopulateUV2(IAccessorArray<SysVector2> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].UV2 = accessorArray[i];
		}

		void PopulateUV3(IAccessorArray<SysVector2> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].UV3 = accessorArray[i];
		}

		void PopulateColors(IAccessorArray<SysVector4> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].Color = accessorArray[i];
		}

		void PopulateJointIndices(IAccessorArray<SysVector4> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].JointIndex = accessorArray[i];
		}

		void PopulateJointWeights(IAccessorArray<SysVector4> accessorArray)
		{
			for (var i = 0; i < Math.Min(vertexData.Length, accessorArray.Count); i++)
				vertexData[i].JointWeight = accessorArray[i];
		}
	}

	private void LinkPrimitiveJoints(MeshPrimitive primitive, Skin skin)
	{
		primitive.JointMapEntryCount = (uint) skin.JointsCount;

		for (var i = 0; i < primitive.JointMapEntryCount; i++)
		{
			var skinJoint = skin.Joints[i];
			var jointIndex = JointIndexTranslationMap[skinJoint.LogicalIndex];

			primitive.JointMap[i] = jointIndex;
			JointsByIndex[jointIndex].IsUsedForSkinning = true;
		}
	}

	#endregion

	private static SysVector3 ScalePosition(SysVector3 position)
		// Scales standard glTF meter units to Dread centimeter units
		=> position * 100;

	private static bool IsJointOrEmptyNode(Node node)
		=> node.IsSkinJoint || (
			node.Mesh is null &&
			node.Skin is null &&
			node.Camera is null &&
			node.PunctualLight is null
		);
}