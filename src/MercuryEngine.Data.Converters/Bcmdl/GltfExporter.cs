using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using ImageMagick;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.GameAssets;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.Types.Bcmdl;
using MercuryEngine.Data.Types.Bcmdl.Wrappers;
using MercuryEngine.Data.Types.Bcskla;
using MercuryEngine.Data.Types.Bsmat;
using MercuryEngine.Data.Types.Fields;
using SharpGLTF.Animations;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using AlphaMode = SharpGLTF.Materials.AlphaMode;
using MeshPrimitive = MercuryEngine.Data.Types.Bcmdl.MeshPrimitive;

namespace MercuryEngine.Data.Converters.Bcmdl;

public sealed class GltfExporter(IGameAssetResolver? assetResolver = null) : IDisposable
{
	/// <summary>
	/// Raised when a non-fatal warning is encountered during BCMDL import or glTF export.
	/// </summary>
	public event Action<string>? Warning;

	public IGameAssetResolver? AssetResolver { get; } = assetResolver;

	private Dictionary<string, NodeBuilder>            ArmatureNodeCache      { get; } = [];
	private Dictionary<StrId, NodeBuilder>             ArmatureNodeCacheByCrc { get; } = [];
	private Dictionary<ArmatureJoint, Matrix4x4>       JointMatrixCache       { get; } = [];
	private Dictionary<TextureCacheKey, CachedTexture> TextureCache           { get; } = [];
	private List<IDisposable>                          Disposables            { get; } = [];

	private SceneBuilder? CurrentScene        { get; set; }
	private Armature?     CurrentArmature     { get; set; }
	private VertexData[]? CurrentVertexBuffer { get; set; }
	private ushort[]?     CurrentIndexBuffer  { get; set; }

	#region Public API

	public void LoadBcmdl(Formats.Bcmdl bcmdl, string? sceneName = null)
	{
		// Clean up beforehand, in case anything got left behind from a previous run
		Dispose();

		CurrentScene = new SceneBuilder(sceneName);

		// Build the armature first
		CurrentArmature = bcmdl.GetArmature();

		BuildSceneArmature();

		// Build meshes
		foreach (var bcmdlNode in bcmdl.Nodes)
		{
			if (bcmdlNode?.Mesh is not { VertexBuffer: { } vertexBuffer, IndexBuffer: { } indexBuffer })
				continue;

			CurrentVertexBuffer = vertexBuffer.GetVertices();
			CurrentIndexBuffer = indexBuffer.GetIndices();

			AddMeshInstance(CurrentScene, bcmdlNode);

			CurrentVertexBuffer = null;
			CurrentIndexBuffer = null;
		}
	}

	public void AttachAnimation(string animationName, Bcskla animationContainer)
	{
		AssertScene();

		var index = 0;

		foreach (var boneTrack in animationContainer.Tracks)
		{
			var thisIndex = index++;

			if (!ArmatureNodeCacheByCrc.TryGetValue(boneTrack.BoneName, out var boneNode))
			{
				Warn($"Animation track {thisIndex} referenced unknown bone \"{boneTrack.BoneName}\"");
				continue;
			}

			AttachNodeAnimation(animationName, animationContainer.FrameCount, boneNode, boneTrack);
		}
	}

	public void ExportGltf(string targetFilePath, bool binary = true)
	{
		AssertScene();

		if (binary)
		{
			CurrentScene.ToGltf2().SaveGLB(targetFilePath, new WriteSettings {
				ImageWriting = ResourceWriteMode.BufferView,
			});
		}
		else
		{
			CurrentScene.ToGltf2().SaveGLTF(targetFilePath);
		}
	}

	#endregion

	#region Armature

	private void BuildSceneArmature()
	{
		if (CurrentScene is null || CurrentArmature is null)
			return;

		ArmatureNodeCache.Clear();
		ArmatureNodeCacheByCrc.Clear();
		JointMatrixCache.Clear();

		foreach (var joint in CurrentArmature.RootJoints)
		{
			VisitJoint(joint);
		}

		return;

		void VisitJoint(ArmatureJoint joint, NodeBuilder? parent = null)
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
			ArmatureNodeCacheByCrc[joint.Name] = builder;
			CurrentScene.AddNode(builder).WithName(joint.Name);
		}
	}

	private Matrix4x4 GetArmatureJointMatrix(ArmatureJoint joint)
	{
		if (JointMatrixCache.TryGetValue(joint, out var jointMatrix))
			return jointMatrix;

		var jointPosition = ScalePosition(joint.Transform.Position);

		jointMatrix = MathHelper.CreateXYZRotationMatrix(joint.Transform.Rotation) *
					  Matrix4x4.CreateTranslation(jointPosition) *
					  Matrix4x4.CreateScale(joint.Transform.Scale);

		JointMatrixCache[joint] = jointMatrix;

		return jointMatrix;
	}

	#endregion

	#region Mesh Instances

	private void AddMeshInstance(SceneBuilder scene, MeshNode bcmdlNode)
	{
		if (bcmdlNode.Mesh is not { Primitives: { } primitives } mesh)
			return;

		// Create and populate the MaterialBuilder
		var materialBuilder = new MaterialBuilder(bcmdlNode.Material?.Name);

		if (bcmdlNode.Material?.Path is { } materialPath && TryLoadMaterial(materialPath, out var material))
			AssignMaterials(materialBuilder, material);

		// Add a mesh for each primitive (joint maps are stored per-primitive in BCMDL, but per mesh in glTF,
		// so in order for the per-vertex indices to line up, the primitives have to be in separate meshes)
		var index = 0;

		foreach (var primitive in primitives)
		{
			if (primitive is null || !TryCreateMeshBuilder(bcmdlNode, bcmdlNode.Id?.Name, out var meshBuilder))
				continue;

			FillPrimitive(bcmdlNode, index, meshBuilder, materialBuilder, primitive);

			// Add primitive to the scene, either as skinned or rigid (depending on if it referenced any joints)
			if (mesh.IsSkinned() && primitive.JointMap.Length > 0)
			{
				// Skinned mesh
				AddSkinnedMesh(scene, bcmdlNode, primitive, meshBuilder);
			}
			else
			{
				// Rigid mesh
				var meshInstanceMatrix = GetMeshTransformMatrix(bcmdlNode);
				NodeBuilder meshParent;

				// Unskinned meshes sometimes have a joint named the same as the mesh, which should be the mesh parent
				if (bcmdlNode.Id?.Name is { } meshName && ArmatureNodeCache.TryGetValue(meshName, out var parentJointNode))
					meshParent = parentJointNode;
				else
					meshParent = new NodeBuilder(bcmdlNode.Id?.Name);

				scene.AddRigidMesh(meshBuilder, meshParent, meshInstanceMatrix).WithName(bcmdlNode.Id?.Name);
			}

			index++;
		}
	}

	private static bool TryCreateMeshBuilder(MeshNode node, string? name, [NotNullWhen(true)] out IMeshBuilder<MaterialBuilder>? meshBuilder)
	{
		if (node is not { Mesh: { VertexBuffer: { } vertexBuffer } mesh })
		{
			meshBuilder = null;
			return false;
		}

		var geometryType = vertexBuffer.GetGeometryType();
		var materialType = vertexBuffer.GetMaterialType();
		var isSkinned = mesh.IsSkinned();

		// Holy hell
		meshBuilder = ( geometryType, materialType, isSkinned ) switch {
			// Non-skinned
			//   Position-only vertices
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPosition, VertexColor1Texture3>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPosition, VertexColor1Texture2>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPosition, VertexColor1Texture1>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Texture3, false)       => new MeshBuilder<VertexPosition, VertexTexture3>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Texture2, false)       => new MeshBuilder<VertexPosition, VertexTexture2>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Texture1, false)       => new MeshBuilder<VertexPosition, VertexTexture1>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1, false)         => new MeshBuilder<VertexPosition, VertexColor1>(name),
			(BcmdlGeometryType.Position, _, false)                                => new MeshBuilder<VertexPosition, VertexEmpty>(name),
			//   Position-Normal vertices
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Texture3, false)       => new MeshBuilder<VertexPositionNormal, VertexTexture3>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Texture2, false)       => new MeshBuilder<VertexPositionNormal, VertexTexture2>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Texture1, false)       => new MeshBuilder<VertexPositionNormal, VertexTexture1>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1, false)         => new MeshBuilder<VertexPositionNormal, VertexColor1>(name),
			(BcmdlGeometryType.PositionNormal, _, false)                                => new MeshBuilder<VertexPositionNormal, VertexEmpty>(name),
			//   Position-Normal-Tangent vertices
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture3>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture2>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture1>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Texture3, false)       => new MeshBuilder<VertexPositionNormalTangent, VertexTexture3>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Texture2, false)       => new MeshBuilder<VertexPositionNormalTangent, VertexTexture2>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Texture1, false)       => new MeshBuilder<VertexPositionNormalTangent, VertexTexture1>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1, false)         => new MeshBuilder<VertexPositionNormalTangent, VertexColor1>(name),
			(BcmdlGeometryType.PositionNormalTangent, _, false)                                => new MeshBuilder<VertexPositionNormalTangent, VertexEmpty>(name),

			// Skinned
			//   Position-only vertices
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPosition, VertexColor1Texture3, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPosition, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPosition, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Texture3, true)       => new MeshBuilder<VertexPosition, VertexTexture3, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Texture2, true)       => new MeshBuilder<VertexPosition, VertexTexture2, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Texture1, true)       => new MeshBuilder<VertexPosition, VertexTexture1, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1, true)         => new MeshBuilder<VertexPosition, VertexColor1, VertexJoints4>(name),
			(BcmdlGeometryType.Position, _, true)                                => new MeshBuilder<VertexPosition, VertexEmpty, VertexJoints4>(name),
			//   Position-Normal vertices
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Texture3, true)       => new MeshBuilder<VertexPositionNormal, VertexTexture3, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Texture2, true)       => new MeshBuilder<VertexPositionNormal, VertexTexture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Texture1, true)       => new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1, true)         => new MeshBuilder<VertexPositionNormal, VertexColor1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, _, true)                                => new MeshBuilder<VertexPositionNormal, VertexEmpty, VertexJoints4>(name),
			//   Position-Normal-Tangent vertices
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture3, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Texture3, true)       => new MeshBuilder<VertexPositionNormalTangent, VertexTexture3, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Texture2, true)       => new MeshBuilder<VertexPositionNormalTangent, VertexTexture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Texture1, true)       => new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1, true)         => new MeshBuilder<VertexPositionNormalTangent, VertexColor1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, _, true)                                => new MeshBuilder<VertexPositionNormalTangent, VertexEmpty, VertexJoints4>(name),

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

			var jointMatrix = primitive.SkinningType == SkinningType.Rigid ? Matrix4x4.Identity : armatureJointNode.WorldMatrix;
			var inverseBindMatrix = SkinnedTransform.CalculateInverseBinding(meshMatrix, jointMatrix);

			bindJoints[index++] = ( armatureJointNode, inverseBindMatrix );
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

		var materialExtras = materialBuilder.Extras ??= new JsonObject();

		materialExtras[GltfProperties.ShaderPath] = material.ShaderPath;

		foreach (var shaderStage in material.ShaderStages)
		{
			// Populate the material's Extras field with uniforms
			foreach (var uniform in shaderStage.Uniforms)
				materialExtras[uniform.Name] = ConvertUniformValuesToJson(uniform);

			// Map samplers/textures to glTF channels
			foreach (var sampler in shaderStage.Samplers.OrderBy(s => s.Index))
			{
				// Store the texture path as an extra property named after the sampler.
				// This can help round-trip more specialized materials, or use existing
				// game materials when importing.
				materialExtras[GltfProperties.SamplerTexturePath(sampler.Name)] = sampler.TexturePath;

				if (!TryGetKnownChannel(sampler.Name, out var channel))
				{
					Warn($"Not sure how to map sampler \"{sampler.Name}\" to a glTF texture channel. This sampler will be unmapped in the glTF output.");
					continue;
				}

				if (LoadAndPrepareTexture(channel, sampler.TexturePath) is not { } cachedTexture)
					continue;

				var (mainImageBuilder, subImageBuilder) = cachedTexture;
				var channelBuilder = materialBuilder.UseChannel(channel);
				var textureBuilder = channelBuilder
					.UseTexture()
					.WithPrimaryImage(mainImageBuilder)
					.WithSampler(
						sampler.TilingModeU.ToTextureWrapMode(),
						sampler.TilingModeV.ToTextureWrapMode(),
						sampler.MagnificationFilter.ToTextureMipMapFilter(),
						sampler.MinificationFilter.ToTextureInterpolationFilter()
					);

				// Store the BCTEX path on an extra field on the texture, which can allow us
				// to reuse an existing BCTEX if re-importing the model later.
				textureBuilder.Extras = new JsonObject {
					[GltfProperties.TexturePath] = sampler.TexturePath,
				};

				if (channel == KnownChannel.BaseColor && subImageBuilder != null)
					materialBuilder.WithEmissive(subImageBuilder, Vector3.One);
				else if (channel == KnownChannel.Normal)
					materialBuilder.WithChannelParam(KnownChannel.Normal, KnownProperty.NormalScale, 1f);
				else if (channel == KnownChannel.MetallicRoughness)
					materialBuilder.WithOcclusion(mainImageBuilder); // Same texture for both - Red = AO, Green/Blue = M/R
			}
		}
	}

	private static JsonArray ConvertUniformValuesToJson(UniformParameter uniform)
		=> uniform.Type switch {
			UniformParameter.TypeFloat       => new JsonArray(uniform.FloatValues.Select(v => JsonValue.Create(v)).ToArray()),
			UniformParameter.TypeSignedInt   => new JsonArray(uniform.SignedIntValues.Select(v => JsonValue.Create(v)).ToArray()),
			UniformParameter.TypeUnsignedInt => new JsonArray(uniform.UnsignedIntValues.Select(v => JsonValue.Create(v)).ToArray()),
			_                                => [],
		};

	private static bool TryGetKnownChannel(string textureName, out KnownChannel channel)
	{
		bool valid;

		// TODO: The "textureN" ones aren't always correct, and there can be duplicates (e.g. "texBaseColor1")
		//  This probably needs to be reworked, and it will likely never be 100% correct due to the shaders all being different.
		( valid, channel ) = textureName switch {
			KnownSamplerNames.BaseColor  => ( true, KnownChannel.BaseColor ),
			KnownSamplerNames.Attributes => ( true, KnownChannel.MetallicRoughness ), // This is technically correct, but must be split as it also contains occlusion
			KnownSamplerNames.Normals    => ( true, KnownChannel.Normal ),
			_                            => ( false, (KnownChannel) ( -1 ) ),
		};

		return valid;
	}

	private CachedTexture? LoadAndPrepareTexture(KnownChannel forChannel, string bctexPath)
	{
		var cacheKey = new TextureCacheKey(forChannel, bctexPath);

		if (TextureCache.TryGetValue(cacheKey, out var cachedTexture))
			return cachedTexture;

		if (!TryLoadTexture(bctexPath, out var bctex) || bctex is not { Textures.Count: 1 })
		{
			Warn($"Texture \"{bctexPath}\" not found or did not contain exactly one texture image");
			return null;
		}

		var inputTexture = bctex.Textures.Single().ToImage(isSrgb: bctex.IsSrgb);
		var textureName = Path.GetFileNameWithoutExtension(bctexPath);
		var subTextureName = textureName;
		MagickImage mainImage;
		MagickImage? subImage = null;

		if (forChannel == KnownChannel.BaseColor)
		{
			( mainImage, subImage ) = TextureConverter.SeparateBaseColorAndEmissive(inputTexture);
			subTextureName = ReplaceOrAddSuffix(textureName, "_bc", "_em");
		}
		else if (forChannel == KnownChannel.MetallicRoughness)
		{
			mainImage = inputTexture;
		}
		else if (forChannel == KnownChannel.Normal)
		{
			mainImage = TextureConverter.ConvertNormalMapFromDread(inputTexture);
		}
		else
		{
			mainImage = inputTexture;
		}

		var mainMemoryImage = ToMemoryImage(mainImage);
		var mainImageBuilder = ImageBuilder.From(mainMemoryImage, textureName);
		var subMemoryImage = default(MemoryImage);
		var subImageBuilder = default(ImageBuilder);

		Disposables.Add(mainImage);

		if (subImage != null)
		{
			Disposables.Add(subImage);
			subMemoryImage = ToMemoryImage(subImage);
			subImageBuilder = ImageBuilder.From(subMemoryImage, subTextureName);
		}

		cachedTexture = new CachedTexture(mainImageBuilder, subImageBuilder);
		TextureCache[cacheKey] = cachedTexture;
		return cachedTexture;
	}

	private bool TryLoadMaterial(string materialPath, [NotNullWhen(true)] out Bsmat? material)
	{
		material = null;

		if (AssetResolver is null)
			return false;

		var materialAsset = AssetResolver.GetAsset(materialPath);

		if (!materialAsset.Exists)
			return false;

		material = materialAsset.ReadAs<Bsmat>(useExistingOutput: true);
		return true;
	}

	private bool TryLoadTexture(string texturePath, [NotNullWhen(true)] out Bctex? texture)
	{
		texture = null;

		if (AssetResolver is null)
			return false;

		// Textures are referenced without the first "textures" folder throughout the game
		var textureAsset = AssetResolver.GetAsset($"textures/{texturePath}");

		if (!textureAsset.Exists)
			return false;

		texture = textureAsset.ReadAs<Bctex>(useExistingOutput: true);
		return true;
	}

	private static MemoryImage ToMemoryImage(MagickImage image)
	{
		using var outputStream = new MemoryStream();

		image.Write(outputStream, MagickFormat.Png00);

		return new MemoryImage(outputStream.ToArray());
	}

	private static string ReplaceOrAddSuffix(string input, string oldSuffix, string newSuffix)
	{
		input = input.Replace(oldSuffix, "");

		return $"{input}{newSuffix}";
	}

	#endregion

	#region Primitives

	private void FillPrimitive(MeshNode meshNode, int primitiveIndex, IMeshBuilder<MaterialBuilder> meshBuilder, MaterialBuilder materialBuilder, MeshPrimitive meshPrimitive)
	{
		if (CurrentVertexBuffer is not { } vertices || CurrentIndexBuffer is not { } indices)
			return;

		if (meshPrimitive.IndexCount % 3 != 0)
		{
			Warn($"Primitive {primitiveIndex} of mesh \"{meshNode.Id?.Name}\" had an invalid number of indices: {meshPrimitive.IndexCount} (must be divisible by 3)");
			return;
		}

		var startIndex = meshPrimitive.IndexOffset;
		var endIndex = startIndex + meshPrimitive.IndexCount - 1;

		if (endIndex >= indices.Length)
		{
			Warn($"Primitive {primitiveIndex} of mesh \"{meshNode.Id?.Name}\" had too many indices (last IndexBuffer index was {endIndex}, but IndexBuffer only has {indices.Length} indices)");
			return;
		}

		var primitiveBuilder = meshBuilder.UsePrimitive(materialBuilder);

		for (var i = startIndex; i <= endIndex - 2; i += 3)
		{
			var triIndexA = indices[i];
			var triIndexB = indices[i + 1];
			var triIndexC = indices[i + 2];

			if (triIndexA >= vertices.Length || triIndexB >= vertices.Length || triIndexC >= vertices.Length)
			{
				var offendingIndex = Math.Max(triIndexA, Math.Max(triIndexB, triIndexC));
				Warn($"Primitive {primitiveIndex} of mesh \"{meshNode.Id?.Name}\" had an invalid triangle (referenced vertex {offendingIndex}, but vertex buffer only has {vertices.Length} vertices)");
				return;
			}

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
			=> ( vertexData.Color, vertexData.UV1, vertexData.UV2, vertexData.UV3 ) switch {
				({ } color, { } uv1, { } uv2, { } uv3) => new VertexColor1Texture3(color, uv1, uv2, uv3),
				({ } color, { } uv1, { } uv2, null)    => new VertexColor1Texture2(color, uv1, uv2),
				({ } color, { } uv1, null, null)       => new VertexColor1Texture1(color, uv1),
				({ } color, null, null, null)          => new VertexColor1(color),
				(null, { } uv1, { } uv2, { } uv3)      => new VertexTexture3(uv1, uv2, uv3),
				(null, { } uv1, { } uv2, null)         => new VertexTexture2(uv1, uv2),
				(null, { } uv1, null, null)            => new VertexTexture1(uv1),
				_                                      => new VertexEmpty(),
			};

		IVertexSkinning CreateSkinning()
			=> ( vertexData.JointIndex, vertexData.JointWeight ) switch {
				({ } jointIndex, { } jointWeight) => new VertexJoints4(SparseWeight8.Create(jointIndex, jointWeight)),
				_                                 => new VertexEmpty(),
			};
	}

	#endregion

	#region Animations

	private static void AttachNodeAnimation(string trackName, float frameCount, NodeBuilder boneNode, BoneTrack boneTrack)
	{
		FillAnimationTrack(boneNode.UseTranslation(trackName), frameCount, boneTrack.Values.Position, 0.01f /* cm to m */);
		FillAnimationTrack(boneNode.UseRotation(trackName), frameCount, boneTrack.Values.Rotation);
		FillAnimationTrack(boneNode.UseScale(trackName), frameCount, boneTrack.Values.Scale);
	}

	private static void FillAnimationTrack(CurveBuilder<Vector3> curveBuilder, float frameCount, AnimatableVector trackVector, float valueScale = 1f)
	{
		// BCSKLA stores each vector component as its own track, so we need to use some funky logic to
		// blend the three component tracks into a single Vector3 curve.

		// All the CurveBuilder classes are internal, so we have to use this convoluted method to create an empty CurveBuilder<Vector3>...
		var xCurve = CreateVectorCurveBuilder();
		var yCurve = CreateVectorCurveBuilder();
		var zCurve = CreateVectorCurveBuilder();

		FillAnimationTrack(xCurve, frameCount, trackVector.X, out var xRates, valueScale);
		FillAnimationTrack(yCurve, frameCount, trackVector.Y, out var yRates, valueScale);
		FillAnimationTrack(zCurve, frameCount, trackVector.Z, out var zRates, valueScale);

		var allFrameKeys = xCurve.Keys.Concat([..yCurve.Keys, ..zCurve.Keys]).Distinct().Order().ToList();

		foreach (var frameKey in allFrameKeys)
		{
			var xValue = xCurve.GetPoint(frameKey).X;
			var yValue = yCurve.GetPoint(frameKey).X;
			var zValue = zCurve.GetPoint(frameKey).X;
			var valueVector = new Vector3(xValue, yValue, zValue);
			var xRate = xRates.GetValueOrDefault(frameKey);
			var yRate = yRates.GetValueOrDefault(frameKey);
			var zRate = zRates.GetValueOrDefault(frameKey);
			var rateVector = new Vector3(xRate, yRate, zRate);

			curveBuilder.SetPoint(frameKey, valueVector, isLinear: false);
			curveBuilder.SetIncomingTangent(frameKey, -rateVector);
			curveBuilder.SetOutgoingTangent(frameKey, rateVector);
		}
	}

	private static void FillAnimationTrack(CurveBuilder<Quaternion> curveBuilder, float frameCount, AnimatableVector trackVector)
	{
		var vectorCurveBuilder = CreateVectorCurveBuilder();

		FillAnimationTrack(vectorCurveBuilder, frameCount, trackVector);

		// Merge all rates manually since we can't sample those
		var allFrameRates = new Dictionary<float, Vector3>();

		foreach (var (frame, keyframeValues) in trackVector.X.GetValues())
		{
			var frameKey = GetFrameKey(frameCount, frame);
			var frameVector = allFrameRates.GetValueOrDefault(frameKey);

			frameVector.X = keyframeValues.Rate;
			allFrameRates[frameKey] = frameVector;
		}

		foreach (var (frame, keyframeValues) in trackVector.Y.GetValues())
		{
			var frameKey = GetFrameKey(frameCount, frame);
			var frameVector = allFrameRates.GetValueOrDefault(frameKey);

			frameVector.Y = keyframeValues.Rate;
			allFrameRates[frameKey] = frameVector;
		}

		foreach (var (frame, keyframeValues) in trackVector.Z.GetValues())
		{
			var frameKey = GetFrameKey(frameCount, frame);
			var frameVector = allFrameRates.GetValueOrDefault(frameKey);

			frameVector.Z = keyframeValues.Rate;
			allFrameRates[frameKey] = frameVector;
		}

		// Convert the vector samples to Quaternion samples using Pitch/Roll/Yaw rotation
		foreach (var frameKey in vectorCurveBuilder.Keys)
		{
			var frameAngles = vectorCurveBuilder.GetPoint(frameKey);
			var frameMatrix = MathHelper.CreateXYZRotationMatrix(frameAngles.X, frameAngles.Y, frameAngles.Z);
			var frameQuat = Quaternion.CreateFromRotationMatrix(frameMatrix);
			var frameRates = allFrameRates.GetValueOrDefault(frameKey);
			var incomingTangentMatrix = MathHelper.CreateXYZRotationMatrix(-frameRates.X, -frameRates.Y, -frameRates.Z);
			var outgoingTangentMatrix = MathHelper.CreateXYZRotationMatrix(frameRates.X, frameRates.Y, frameRates.Z);
			var incomingTangentQuat = Quaternion.CreateFromRotationMatrix(incomingTangentMatrix);
			var outgoingTangentQuat = Quaternion.CreateFromRotationMatrix(outgoingTangentMatrix);

			curveBuilder.SetPoint(frameKey, frameQuat, isLinear: false);
			curveBuilder.SetIncomingTangent(frameKey, incomingTangentQuat);
			curveBuilder.SetOutgoingTangent(frameKey, outgoingTangentQuat);
		}
	}

	private static void FillAnimationTrack(CurveBuilder<Vector3> curveBuilder, float frameCount, AnimatableValue trackValue, out Dictionary<float, float> rates, float valueScale = 1f)
	{
		rates = new Dictionary<float, float>();

		if (trackValue.IsConstant)
		{
			curveBuilder.SetPoint(0f, new Vector3(trackValue.ConstantValue * valueScale, 0f, 0f), isLinear: false);
			curveBuilder.SetPoint(1f, new Vector3(trackValue.ConstantValue * valueScale, 0f, 0f), isLinear: false);
		}
		else
		{
			foreach (var (frame, keyframeValues) in trackValue.GetValues())
			{
				var frameKey = GetFrameKey(frameCount, frame);

				curveBuilder.SetPoint(frameKey, new Vector3(keyframeValues.Value * valueScale, 0f, 0f), isLinear: false);
				curveBuilder.SetIncomingTangent(frameKey, new Vector3(-keyframeValues.Rate * valueScale, 0f, 0f));
				curveBuilder.SetOutgoingTangent(frameKey, new Vector3(keyframeValues.Rate * valueScale, 0f, 0f));
				rates[frameKey] = keyframeValues.Rate * valueScale;
			}
		}
	}

	private static CurveBuilder<Vector3> CreateVectorCurveBuilder()
		// All the CurveBuilder classes are internal, so we have to use this convoluted method to create an empty CurveBuilder<Vector3>...
		=> new NodeBuilder().UseTranslation().UseTrackBuilder("dummy");

	private static float GetFrameKey(float frameCount, ushort frameIndex)
		=> frameIndex / frameCount;

	#endregion

	[MemberNotNull(nameof(CurrentScene))]
	private void AssertScene()
	{
		if (CurrentScene is null)
			throw new InvalidOperationException("A BCMDL has not yet been loaded");
	}

	[OverloadResolutionPriority(1)]
	private void Warn(FormattableString message)
		=> Warning?.Invoke(message.ToString());

	private void Warn(string message)
		=> Warning?.Invoke(message);

	private static Vector3 ScalePosition(Vector3 rawPosition)
		// Scale cm to meters
		=> new(rawPosition.X / 100f, rawPosition.Y / 100f, rawPosition.Z / 100f);

	#region IDisposable

	public void Dispose()
	{
		ArmatureNodeCache.Clear();
		ArmatureNodeCacheByCrc.Clear();
		JointMatrixCache.Clear();
		TextureCache.Clear();

		foreach (var disposable in Disposables)
			disposable.Dispose();

		Disposables.Clear();
	}

	#endregion

	#region Helper Types

	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	private record struct TextureCacheKey(KnownChannel Channel, string TexturePath);

	private sealed record CachedTexture(ImageBuilder MainImageBuilder, ImageBuilder? SubImageBuilder);

	#endregion
}