using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.Bcmdl;
using MercuryEngine.Data.Types.Bcmdl.Wrappers;
using MercuryEngine.Data.Types.Bsmat;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using SkiaSharp;
using AlphaMode = SharpGLTF.Materials.AlphaMode;
using MeshPrimitive = MercuryEngine.Data.Types.Bcmdl.MeshPrimitive;

namespace MercuryEngine.Data.Converters.Bcmdl;

public class GltfExporter(IMaterialResolver? materialResolver = null)
{
	private const string TextureNameBaseColor  = "texBaseColor";
	private const string TextureNameNormals    = "texNormals";
	private const string TextureNameAttributes = "texAttributes";

	public IMaterialResolver? MaterialResolver { get; } = materialResolver;

	private Dictionary<string, NodeBuilder>      ArmatureNodeCache { get; } = [];
	private Dictionary<ArmatureJoint, Matrix4x4> JointMatrixCache  { get; } = [];

	private Armature?     CurrentArmature     { get; set; }
	private VertexData[]? CurrentVertexBuffer { get; set; }
	private ushort[]?     CurrentIndexBuffer  { get; set; }

	public void ExportGltf(Formats.Bcmdl bcmdl, string targetFilePath, bool binary = true, string? sceneName = null)
	{
		try
		{
			var scene = new SceneBuilder(sceneName);

			// Build the armature first
			CurrentArmature = bcmdl.GetArmature();

			BuildArmature(scene, CurrentArmature);

			// Build meshes
			foreach (var bcmdlNode in bcmdl.Nodes)
			{
				if (bcmdlNode?.Mesh is not { VertexBuffer: { } vertexBuffer, IndexBuffer: { } indexBuffer })
					continue;

				CurrentVertexBuffer = vertexBuffer.GetVertices();
				CurrentIndexBuffer = indexBuffer.GetIndices();

				AddMeshInstance(scene, bcmdlNode);

				CurrentVertexBuffer = null;
				CurrentIndexBuffer = null;
			}

			if (binary)
			{
				scene.ToGltf2().SaveGLB(targetFilePath, new WriteSettings {
					ImageWriting = ResourceWriteMode.BufferView,
				});
			}
			else
			{
				scene.ToGltf2().SaveGLTF(targetFilePath);
			}
		}
		finally
		{
			// Clean up
			ArmatureNodeCache.Clear();
			JointMatrixCache.Clear();
			CurrentArmature = null;
			CurrentVertexBuffer = null;
			CurrentIndexBuffer = null;
		}
	}

	private void BuildArmature(SceneBuilder scene, Armature armature)
	{
		ArmatureNodeCache.Clear();
		JointMatrixCache.Clear();

		foreach (var joint in armature.RootJoints)
		{
			var builder = VisitJoint(joint);

			scene.AddNode(builder).WithName(joint.Name);
		}

		return;

		NodeBuilder VisitJoint(ArmatureJoint joint, NodeBuilder? parent = null)
		{
			var builder = new NodeBuilder(joint.Name);

			// Add the node to its parent (if applicable)
			parent?.AddNode(builder);

			// Apply the joint's transform
			builder.LocalMatrix = GetArmatureJointMatrix(joint);

			// Recursively visit the joint's children
			foreach (var child in joint.Children)
				VisitJoint(child, builder);

			ArmatureNodeCache[joint.Name] = builder;
			return builder;
		}
	}

	private Matrix4x4 GetArmatureJointMatrix(ArmatureJoint joint)
	{
		if (JointMatrixCache.TryGetValue(joint, out var jointMatrix))
			return jointMatrix;

		var jointPosition = ScalePosition(joint.Transform.Position);

		jointMatrix = CreateRotationMatrix(joint.Transform.Rotation) *
					  Matrix4x4.CreateTranslation(jointPosition) *
					  Matrix4x4.CreateScale(joint.Transform.Scale);

		JointMatrixCache[joint] = jointMatrix;

		return jointMatrix;
	}

	#region Mesh Instances

	private void AddMeshInstance(SceneBuilder scene, MeshNode bcmdlNode)
	{
		if (bcmdlNode.Mesh is not { Primitives: { } primitives })
			return;

		// Create and populate the MaterialBuilder
		var materialBuilder = new MaterialBuilder(bcmdlNode.Material?.Name);

		if (bcmdlNode.Material?.Path is { } materialPath && MaterialResolver?.LoadMaterial(materialPath) is { } material)
			AssignMaterials(materialBuilder, material);

		// Add a mesh for each primitive (joint maps are stored per-primitive in BCMDL, but per mesh in glTF,
		// so in order for the per-vertex indices to line up, the primitives have to be in separate meshes)
		foreach (var primitive in primitives)
		{
			if (primitive is null || !TryCreateMeshBuilder(bcmdlNode, bcmdlNode.Id?.Name, out var meshBuilder))
				continue;

			FillPrimitive(meshBuilder, materialBuilder, primitive);

			// Add primitive to the scene, either as skinned or rigid (depending on if it referenced any joints)
			if (primitive.JointMap.Length > 0)
			{
				// Skinned mesh
				AddSkinnedMesh(scene, bcmdlNode, primitive, meshBuilder);
			}
			else
			{
				// Rigid mesh
				var meshNodeBuilder = new NodeBuilder(bcmdlNode.Id?.Name) {
					LocalMatrix = GetMeshTransformMatrix(bcmdlNode),
				};

				scene.AddRigidMesh(meshBuilder, meshNodeBuilder).WithName(bcmdlNode.Id?.Name);
			}
		}
	}

	private static bool TryCreateMeshBuilder(MeshNode node, string? name, [NotNullWhen(true)] out IMeshBuilder<MaterialBuilder>? meshBuilder)
	{
		if (node is not { Material: { } material, Mesh: { VertexBuffer: { } vertexBuffer } mesh })
		{
			meshBuilder = null;
			return false;
		}

		var geometryType = vertexBuffer.GetGeometryType();
		var materialType = material.GetMaterialType();
		var isSkinned = mesh.IsSkinned();

		meshBuilder = ( geometryType, materialType, isSkinned ) switch {
			// Non-skinned
			//   Position-only vertices
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1, false)         => new MeshBuilder<VertexPosition, VertexColor1>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPosition, VertexColor1Texture1>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPosition, VertexColor1Texture2>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPosition, VertexColor1Texture3>(name),
			//   Position-Normal vertices
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1, false)         => new MeshBuilder<VertexPositionNormal, VertexColor1>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3>(name),
			//   Position-Normal-Tangent vertices
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1, false)         => new MeshBuilder<VertexPositionNormalTangent, VertexColor1>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture1>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture2>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture3>(name),

			// Skinned
			//   Position-only vertices
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1, true)         => new MeshBuilder<VertexPosition, VertexColor1, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPosition, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPosition, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPosition, VertexColor1Texture3, VertexJoints4>(name),
			//   Position-Normal vertices
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1, true)         => new MeshBuilder<VertexPositionNormal, VertexColor1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3, VertexJoints4>(name),
			//   Position-Normal-Tangent vertices
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1, true)         => new MeshBuilder<VertexPositionNormalTangent, VertexColor1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture3, VertexJoints4>(name),

			_ => null,
		};
		return meshBuilder != null;
	}

	private void AddSkinnedMesh(SceneBuilder scene, MeshNode meshNode, MeshPrimitive primitive, IMeshBuilder<MaterialBuilder> meshBuilder)
	{
		if (CurrentArmature is not { } armature)
			return;

		var meshMatrix = GetMeshTransformMatrix(meshNode);
		var bindJoints = new (NodeBuilder, Matrix4x4)[primitive.JointMap.Length];
		var index = 0;

		foreach (var jointId in primitive.JointMap)
		{
			if (jointId >= armature.AllJoints.Count)
				throw new ApplicationException($"Mesh \"{meshNode.Id?.Name}\" referenced invalid joint index \"{jointId}\"");

			var armatureJoint = armature.AllJoints[(int) jointId];

			if (!ArmatureNodeCache.TryGetValue(armatureJoint.Name, out var armatureJointNode))
				throw new ApplicationException($"Armature node not found for joint \"{armatureJoint.Name}\"!");

			var jointMatrix = primitive.SkinningType == SkinningType.WholeMeshTransform ? Matrix4x4.Identity : armatureJointNode.WorldMatrix;
			var bindMatrix = SkinnedTransform.CalculateInverseBinding(meshMatrix, jointMatrix);

			bindJoints[index++] = ( armatureJointNode, bindMatrix );
		}

		scene.AddSkinnedMesh(meshBuilder, bindJoints);
	}

	private static Matrix4x4 GetMeshTransformMatrix(MeshNode node)
	{
		var transformMatrix = Matrix4x4.CreateTranslation(node.Mesh!.Translation);

		// Scale the translation appropriately
		transformMatrix.Translation = ScalePosition(transformMatrix.Translation);

		return transformMatrix;
	}

	#endregion

	#region Materials

	private void AssignMaterials(MaterialBuilder materialBuilder, Bsmat material)
	{
		materialBuilder.WithAlpha(material.AlphaState.Enabled ? AlphaMode.BLEND : AlphaMode.OPAQUE, material.AlphaState.Threshold);

		foreach (var shaderStage in material.ShaderStages)
		foreach (var sampler in shaderStage.Samplers.OrderBy(s => s.Index))
		{
			if (!TryGetKnownChannel(sampler.Name, out var channel))
				continue;

			using var inputTexture = MaterialResolver?.LoadTexture(sampler.TexturePath);

			if (inputTexture is null)
				continue;

			var textureName = Path.GetFileNameWithoutExtension(sampler.TexturePath);
			SKBitmap mainTexture;
			SKBitmap? subTexture = null;

			if (channel == KnownChannel.BaseColor)
				( mainTexture, subTexture ) = TextureConverter.SeparateBaseColorAndEmissive(inputTexture);
			else if (channel == KnownChannel.MetallicRoughness)
				( mainTexture, subTexture ) = TextureConverter.SeparateMetallicRoughnessAndOcclusion(inputTexture);
			else if (channel == KnownChannel.Normal)
				mainTexture = TextureConverter.ConvertNormalMap(inputTexture);
			else
				mainTexture = inputTexture;

			using (mainTexture)
			using (subTexture)
			{
				var mainImage = ToMemoryImage(mainTexture);
				var mainImageBuilder = ImageBuilder.From(mainImage, textureName);
				var subImage = default(MemoryImage);
				var subImageBuilder = default(ImageBuilder);
				var channelBuilder = materialBuilder.UseChannel(channel);

				if (subTexture != null)
				{
					subImage = ToMemoryImage(subTexture);
					subImageBuilder = ImageBuilder.From(subImage, $"{textureName}.2");
				}

				channelBuilder
					.UseTexture()
					.WithPrimaryImage(mainImageBuilder)
					.WithSampler(GetWrapMode(sampler.TilingModeU), GetWrapMode(sampler.TilingModeV), GetMipMapFilter(sampler.MagnificationFilter), GetInterpolationFilter(sampler.MinificationFilter));

				if (channel == KnownChannel.BaseColor && subImageBuilder != null)
				{
					materialBuilder.WithEmissive(subImageBuilder, Vector3.One);
				}
				else if (channel == KnownChannel.Normal)
				{
					materialBuilder.WithChannelParam(KnownChannel.Normal, KnownProperty.NormalScale, 1f);
				}
				else if (channel == KnownChannel.MetallicRoughness && subImageBuilder != null)
				{
					materialBuilder.WithOcclusion(subImageBuilder);
				}
			}
		}
	}

	private static bool TryGetKnownChannel(string textureName, out KnownChannel channel)
	{
		bool valid;

		( valid, channel ) = textureName switch {
			TextureNameBaseColor  => ( true, KnownChannel.BaseColor ),
			TextureNameNormals    => ( true, KnownChannel.Normal ),
			TextureNameAttributes => ( true, KnownChannel.MetallicRoughness ), // This is technically correct, but must be split as it also contains occlusion
			_                     => ( false, default ),
		};

		return valid;
	}

	private static MemoryImage ToMemoryImage(SKBitmap bitmap)
	{
		using var outputStream = new MemoryStream();

		bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

		return new MemoryImage(outputStream.ToArray());
	}

	private static TextureWrapMode GetWrapMode(TilingMode tilingMode)
		=> tilingMode switch {
			TilingMode.ClampToColor   => TextureWrapMode.CLAMP_TO_EDGE,
			TilingMode.Repeat         => TextureWrapMode.REPEAT,
			TilingMode.MirroredRepeat => TextureWrapMode.MIRRORED_REPEAT,
			_                         => TextureWrapMode.CLAMP_TO_EDGE,
		};

	private static TextureMipMapFilter GetMipMapFilter(FilterMode magnificationFilter)
		=> magnificationFilter switch {
			FilterMode.Nearest           => TextureMipMapFilter.NEAREST,
			FilterMode.Linear            => TextureMipMapFilter.LINEAR,
			FilterMode.NearestMipNearest => TextureMipMapFilter.NEAREST_MIPMAP_NEAREST,
			FilterMode.NearestMipLinear  => TextureMipMapFilter.NEAREST_MIPMAP_LINEAR,
			FilterMode.LinearMipNearest  => TextureMipMapFilter.LINEAR_MIPMAP_NEAREST,
			FilterMode.LinearMipLinear   => TextureMipMapFilter.LINEAR_MIPMAP_LINEAR,
			_                            => TextureMipMapFilter.DEFAULT,
		};

	private static TextureInterpolationFilter GetInterpolationFilter(FilterMode minifactionFilter)
		=> minifactionFilter switch {
			FilterMode.Nearest => TextureInterpolationFilter.NEAREST,
			FilterMode.Linear  => TextureInterpolationFilter.LINEAR,
			_                  => TextureInterpolationFilter.DEFAULT,
		};

	#endregion

	#region Primitives

	private void FillPrimitive(IMeshBuilder<MaterialBuilder> meshBuilder, MaterialBuilder materialBuilder, MeshPrimitive meshPrimitive)
	{
		if (CurrentVertexBuffer is not { } vertices || CurrentIndexBuffer is not { } indices)
			return;

		if (meshPrimitive.IndexCount % 3 != 0)
			// TODO: Need way of communicating warnings to consumer
			return;

		var startIndex = meshPrimitive.IndexOffset;
		var endIndex = startIndex + meshPrimitive.IndexCount - 1;

		if (endIndex >= indices.Length)
			// TODO: Need way of communicating warnings to consumer
			return;

		var primitiveBuilder = meshBuilder.UsePrimitive(materialBuilder);

		for (var i = startIndex; i <= endIndex - 2; i += 3)
		{
			var triIndexA = indices[i];
			var triIndexB = indices[i + 1];
			var triIndexC = indices[i + 2];

			if (triIndexA >= vertices.Length || triIndexB >= vertices.Length || triIndexC >= vertices.Length)
				// TODO: Need way of communicating warnings to consumer
				return;

			var vertexA = vertices[triIndexA];
			var vertexB = vertices[triIndexB];
			var vertexC = vertices[triIndexC];

			var vertexBuilderA = primitiveBuilder.VertexFactory();
			var vertexBuilderB = primitiveBuilder.VertexFactory();
			var vertexBuilderC = primitiveBuilder.VertexFactory();

			FillVertexData(vertexBuilderA, vertexA);
			FillVertexData(vertexBuilderB, vertexB);
			FillVertexData(vertexBuilderC, vertexC);

			primitiveBuilder.AddTriangle(vertexBuilderA, vertexBuilderB, vertexBuilderC);
		}
	}

	private static void FillVertexData(IVertexBuilder vertexBuilder, VertexData vertexData)
	{
		vertexBuilder.SetGeometry(CreateGeometry());
		vertexBuilder.SetMaterial(CreateMaterial());
		vertexBuilder.SetSkinning(CreateSkinning());

		IVertexGeometry CreateGeometry()
			=> ( vertexData.Position, vertexData.Normal, vertexData.Tangent ) switch {
				({ } position, { } normal, { } tangent) => new VertexPositionNormalTangent(ScalePosition(position), normal, tangent),
				({ } position, { } normal, null)        => new VertexPositionNormal(ScalePosition(position), normal),
				({ } position, null, null)              => new VertexPosition(ScalePosition(position)),

				_ => new VertexPosition(),
			};

		IVertexMaterial CreateMaterial()
			=> ( vertexData.UV1, vertexData.UV2, vertexData.UV3 ) switch {
				({ } uv1, { } uv2, { } uv3) => new VertexColor1Texture3(vertexData.Color ?? Vector4.One, uv1, uv2, uv3),
				({ } uv1, { } uv2, null)    => new VertexColor1Texture2(vertexData.Color ?? Vector4.One, uv1, uv2),
				({ } uv1, null, null)       => new VertexColor1Texture1(vertexData.Color ?? Vector4.One, uv1),
				_                           => new VertexColor1(vertexData.Color ?? Vector4.One),
			};

		IVertexSkinning CreateSkinning()
			=> ( vertexData.JointIndex, vertexData.JointWeight ) switch {
				({ } jointIndex, { } jointWeight) => new VertexJoints4(SparseWeight8.Create(jointIndex, jointWeight)),
				_                                 => new VertexEmpty(),
			};
	}

	#endregion

	private static Vector3 ScalePosition(Vector3 rawPosition)
		// Scale cm to meters
		=> new(rawPosition.X / 100f, rawPosition.Y / 100f, rawPosition.Z / 100f);

	private static Matrix4x4 CreateRotationMatrix(Vector3 rotationVector)
		=> Matrix4x4.CreateRotationX(rotationVector.X) *
		   Matrix4x4.CreateRotationY(rotationVector.Y) *
		   Matrix4x4.CreateRotationZ(rotationVector.Z);
}