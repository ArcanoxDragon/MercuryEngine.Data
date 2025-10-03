using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BCnEncoder.Shared;
using ImageMagick;
using MercuryEngine.Data.Converters.Extensions;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.GameAssets;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.TegraTextureLib.ImageProcessing;
using MercuryEngine.Data.Types.Bcmdl;
using MercuryEngine.Data.Types.Bcmdl.Wrappers;
using MercuryEngine.Data.Types.Bsmat;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using MathHelper = MercuryEngine.Data.Core.Utility.MathHelper;
using Material = MercuryEngine.Data.Types.Bcmdl.Material;
using Mesh = MercuryEngine.Data.Types.Bcmdl.Mesh;
using MeshPrimitive = MercuryEngine.Data.Types.Bcmdl.MeshPrimitive;
using SGMaterial = SharpGLTF.Schema2.Material;
using SGMesh = SharpGLTF.Schema2.Mesh;
using SGMeshPrimitive = SharpGLTF.Schema2.MeshPrimitive;
using SysVector2 = System.Numerics.Vector2;
using SysVector3 = System.Numerics.Vector3;
using SysVector4 = System.Numerics.Vector4;
using Vector3 = MercuryEngine.Data.Types.DreadTypes.Vector3;

namespace MercuryEngine.Data.Converters.Bcmdl;

public partial class GltfImporter(IGameAssetResolver assetResolver)
{
	/// <summary>
	/// This shader supports a base color with an emissive layer, normal maps, and PBR metallic/roughness/AO.
	/// </summary>
	private const string InitialDefaultShader = "system/shd/mp_opaque_constant_selfilum.bshdat";

	private int nextUnknownTextureId = 1;

	/// <summary>
	/// Raised when a non-fatal warning is encountered during glTF import.
	/// </summary>
	public event Action<string>? Warning;

	public IGameAssetResolver AssetResolver { get; } = assetResolver;

	/// <summary>
	/// The shader to use for materials without a custom shader path specified.
	/// </summary>
	public string DefaultShader { get; set; } = InitialDefaultShader;

	/// <summary>
	/// Gets or sets the <see cref="TegraTextureLib.ImageProcessing.TextureEncodingOptions"/> to use when encoding model textures.
	/// </summary>
	public TextureEncodingOptions TextureEncodingOptions { get; set; } = new();

	private GltfImportResult?               CurrentResult            { get; set; }
	private Dictionary<int, uint>           JointIndexTranslationMap { get; } = [];
	private Dictionary<uint, Joint>         JointsByIndex            { get; } = [];
	private Dictionary<string, MagickImage> DecodedImages            { get; } = [];
	private List<IDisposable>               Disposables              { get; } = [];

	private Dictionary<Texture, MagickImage>             ConvertedTextureCache { get; } = [];
	private Dictionary<(Texture, Texture?), MagickImage> CombinedTextureCache  { get; } = [];
	private Dictionary<MagickImage, Bctex>               EncodedTextureCache   { get; } = [];

	private ActorType CurrentActorType => CurrentResult?.ActorType ?? throw new InvalidOperationException("A glTF file has not been loaded");
	private string    CurrentActorName => CurrentResult?.ActorName ?? throw new InvalidOperationException("A glTF file has not been loaded");
	private string    CurrentModelName => CurrentResult?.ModelName ?? throw new InvalidOperationException("A glTF file has not been loaded");

	private string CurrentRomFsSubfolder => $"actors/{CurrentActorType.GetRomFsFolder()}/{CurrentActorName}";

	public GltfImportResult ImportGltf(ActorType actorType, string actorName, string sourceFilePath)
	{
		var modelName = Path.GetFileNameWithoutExtension(sourceFilePath);

		return ImportGltf(actorType, actorName, modelName, sourceFilePath);
	}

	public GltfImportResult ImportGltf(ActorType actorType, string actorName, string modelName, string sourceFilePath)
	{
		try
		{
			var targetModel = new Formats.Bcmdl();
			var finalBcmdlPath = $"actors/{actorType.GetRomFsFolder()}/{actorName}/models/{modelName}.bcmdl";
			var bcmdlAsset = AssetResolver.GetAsset(finalBcmdlPath);

			CurrentResult = new GltfImportResult(actorType, actorName, modelName, targetModel, bcmdlAsset);
			JointIndexTranslationMap.Clear();
			JointsByIndex.Clear();
			DecodedImages.Clear();
			Disposables.Clear();
			ConvertedTextureCache.Clear();
			CombinedTextureCache.Clear();
			EncodedTextureCache.Clear();

			var readSettings = new ReadSettings {
				ImageDecoder = DecodeTextureImage,
				Validation = ValidationMode.Skip,
			};
			var modelRoot = ModelRoot.Load(sourceFilePath, readSettings);

			// Create Joints from the scene's skin joints
			foreach (var node in modelRoot.DefaultScene.VisualChildren)
				ImportJointsFromNodeHierarchy(node);

			// Create MeshNodes from the scene's visual children
			var allMeshes = modelRoot.DefaultScene.VisualChildren
				.SelectMany(FindMeshesInNodeHierarchy)
				.ToList();

			ImportMeshes(allMeshes);

			return CurrentResult;
		}
		finally
		{
			// Clean up
			CurrentResult = null;
			JointIndexTranslationMap.Clear();
			JointsByIndex.Clear();
			DecodedImages.Clear();
			ConvertedTextureCache.Clear();
			CombinedTextureCache.Clear();
			EncodedTextureCache.Clear();

			foreach (var disposable in Disposables)
				disposable.Dispose();

			Disposables.Clear();
		}
	}

	private bool DecodeTextureImage(Image image)
	{
		if (string.IsNullOrEmpty(image.Name))
		{
			Warn("Image found without a name - this image will be skipped");
			return false;
		}

		if (image.Content is not { IsPng: true, IsValid: true })
		{
			Warn($"Image \"{image.Name}\" is not a valid PNG image and will be skipped");
			return false;
		}

		try
		{
			MagickImage magickImage;

			using (var imageStream = image.Content.Open())
				magickImage = new MagickImage(imageStream);

			DecodedImages[image.Name] = magickImage;
			Disposables.Add(magickImage);
			return true;
		}
		catch (Exception ex)
		{
			Warn($"Exception decoding image \"{image.Name}\": {ex}");
			return false;
		}
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
			Transform = new Transform {
				Position = translation,
				Rotation = rotation,
				Scale = scale,
			},
		};

		// Add the joint's Transform into the model
		CurrentResult!.Model.Transforms.Add(joint.Transform);

		var jointsInfo = CurrentResult!.Model.JointsInfo ??= new JointsInfo();
		var thisJointIndex = (uint) jointsInfo.Joints.Count;

		// Map the "logical index" of the joint node to its index in the BCMDL
		JointIndexTranslationMap[node.LogicalIndex] = thisJointIndex;
		JointsByIndex[thisJointIndex] = joint;
		jointsInfo.Joints.Add(joint);

		// Recurse children
		foreach (var child in node.VisualChildren)
			ImportJointsFromNodeHierarchyCore(child, node);
	}

	#endregion

	#region Meshes

	private static IEnumerable<NodeWithMesh> FindMeshesInNodeHierarchy(Node node)
	{
		if (node.Mesh is { } mesh)
		{
			var meshName = node.Name ?? mesh.Name;

			// Try and remove a numeric suffix, e.g. ".001" (these will be combined into a single mesh node in BCMDL)
			if (MeshNameRegex.IsMatch(meshName, out var match))
				meshName = match.Groups["Name"].Value;

			yield return new NodeWithMesh(meshName, node, mesh);
		}

		foreach (var child in node.VisualChildren)
		foreach (var result in FindMeshesInNodeHierarchy(child))
			yield return result;
	}

	private void ImportMeshes(List<NodeWithMesh> allMeshes)
	{
		// First, identify all distinct materials and convert them to BSMATs
		var distinctMaterials = allMeshes.SelectMany(m => m.Mesh.Primitives).Select(p => p.Material).Where(m => m != null).Distinct().ToList();

		foreach (var material in distinctMaterials)
			ImportMaterial(material);

		foreach (var (name, node, mesh) in allMeshes)
		{
			// Group the mesh's primitives by material. Primitives with different materials will be in different BCMDL mesh nodes.
			var primitivesByMaterial = mesh.Primitives.GroupBy(p => p.Material);

			foreach (var primitiveGroup in primitivesByMaterial)
				ImportMeshNode(name, node, primitiveGroup.Key, primitiveGroup);
		}
	}

	private void ImportMeshNode(string name, Node node, SGMaterial? material, IEnumerable<SGMeshPrimitive> primitives)
	{
		if (material is null)
		{
			Warn($"Skipping mesh \"{name}\" because it does not have a material applied");
			return;
		}

		var meshNode = new MeshNode {
			Id = CurrentResult!.Model.GetOrCreateNodeId(name),
			Material = CurrentResult!.Model.Materials.SingleOrDefault(m => m?.Name == material.Name),
			Mesh = new Mesh {
				TransformMatrix = Matrix4x4.Identity with {
					Translation = ScalePosition(node.WorldMatrix.Translation),
				},
				Translation = new Vector3(ScalePosition(node.LocalTransform.Translation)),
			},
		};

		var indices = new List<ushort>();
		var vertices = new List<VertexData>();

		foreach (var primitive in primitives)
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

	#region Materials

	private void ImportMaterial(SGMaterial material)
	{
		if (CurrentResult!.Materials.ContainsKey(material.Name))
		{
			Warn($"Skipping duplicate material \"{material.Name}\"");
			return;
		}

		if (!TryGetShaderForMaterial(material, out var shaderLocation, out _)) // TODO: Use shader for reflection info
			// Can't import materials without a valid shader
			return;

		if (!TryGetPrototypeMaterialForShader(shaderLocation, out var prototypeMaterialLocation))
		{
			// Can't import materials without a prototype to start with
			Warn($"Prototype material not found for shader \"{shaderLocation.RelativePath}\" (referenced by material \"{material.Name}\")");
			return;
		}

		// Dread names the actual material files (both the filename, and the name stored in the BSMAT file) as "modelname_materialname"
		var fullMaterialName = $"{CurrentModelName}_{material.Name}";
		var prototypeMaterial = prototypeMaterialLocation.ReadAs<Bsmat>();

		// Create a BCMDL Material resource as well
		var bcmdlMaterial = new Material {
			Name = material.Name,
		};

		// Apply the material being imported onto the prototype
		prototypeMaterial.Name = fullMaterialName;

		foreach (var stage in prototypeMaterial.ShaderStages)
		foreach (var sampler in stage.Samplers)
		{
			Texture? mainTexture;
			Texture? subTexture;

			if (material.Extras.TryGetProperty(GltfProperties.SamplerTexturePath(sampler.Name), out string? bctexPath))
			{
				// Material points to an existing BCTEX for this sampler

				if (string.IsNullOrEmpty(bctexPath) || AssetResolver.TryGetExistingAsset($"textures/{bctexPath}", out _))
					sampler.TexturePath = bctexPath;
				else
					Warn($"Material \"{material.Name}\" referenced non-existent texture \"{bctexPath}\" for sampler \"{sampler.Name}\"");
			}
			else
			{
				// Try to figure out which channel's image to bind to this sampler

				switch (sampler.Name)
				{
					case KnownSamplerNames.BaseColor:
						mainTexture = material.GetDiffuseTexture();
						subTexture = material.FindChannel(nameof(KnownChannel.Emissive))?.Texture;

						if (mainTexture != null)
						{
							BindBaseColorTexture(material, mainTexture, subTexture, sampler);
							bcmdlMaterial.Tex1Name = mainTexture.GetName() ?? string.Empty;
						}

						break;
					case KnownSamplerNames.Attributes:
						mainTexture = material.FindChannel(nameof(KnownChannel.MetallicRoughness))?.Texture;
						subTexture = material.FindChannel(nameof(KnownChannel.Occlusion))?.Texture;

						if (mainTexture != null)
						{
							BindAttributesTexture(material, mainTexture, subTexture, sampler);

							// Attributes are 3 in BCMDL material even though they're 2 in BSMAT...
							// TODO: Not sure if this matters?
							bcmdlMaterial.Tex3Name = mainTexture.GetName() ?? string.Empty;
						}

						break;
					case KnownSamplerNames.Normals:
						mainTexture = material.FindChannel(nameof(KnownChannel.Normal))?.Texture;

						if (mainTexture != null)
						{
							BindNormalsTexture(material, mainTexture, sampler);
							bcmdlMaterial.Tex2Name = mainTexture.GetName() ?? string.Empty;
						}

						break;
					default:
						Warn($"Ignoring unknown sampler \"{sampler.Name}\" in material \"{prototypeMaterialLocation.RelativePath}\" (unknown texture binding)");
						break;
				}
			}
		}

		if (material.Extras is JsonObject extras)
		{
			// Extra properties that start with lowercase "f", "v", or "i" are interpreted as uniforms
			// TODO: Use shader reflection info to figure these out exactly, and map them to the correct stage instead of assuming Fragment

			foreach (var (name, node) in extras)
			{
				if (name.Length == 0 || node is not (JsonValue or JsonArray))
					continue;
				if (name[0] is not ('f' or 'v' or 'i'))
					continue;

				foreach (var fragmentStage in prototypeMaterial.ShaderStages.Where(s => s.Type == ShaderType.Fragment))
				{
					var parameter = fragmentStage.Uniforms.FirstOrDefault(u => u.Name == name);

					if (parameter is null)
					{
						parameter = new UniformParameter { Name = name };
						fragmentStage.Uniforms.Add(parameter);
					}

					if (name[0] is 'f' or 'v')
						parameter.FloatValues = GetValueAsArray<float>();
					else // 'i' for int32
						parameter.SignedIntValues = GetValueAsArray<int>();
				}

				T[] GetValueAsArray<T>()
				{
					T[] values = [];

					if (node is JsonArray array)
						values = array.OfType<JsonValue>().TrySelect<JsonValue, T>((v, out f) => v.TryGetValue(out f)).ToArray();
					else if (node is JsonValue value)
						values = value.TryGetValue<T>(out var v) ? [v] : [];

					return values;
				}
			}
		}

		// Assign a path to the material and store it in results
		var materialAssetPath = $"{CurrentRomFsSubfolder}/models/imats/{fullMaterialName}.bsmat";
		var materialAsset = AssetResolver.GetAsset(materialAssetPath);

		bcmdlMaterial.Path = materialAssetPath;
		CurrentResult!.Model.Materials.Add(bcmdlMaterial);
		CurrentResult!.Materials[material.Name] = ( prototypeMaterial, materialAsset );
	}

	private bool TryGetShaderForMaterial(SGMaterial material, out GameAsset shaderAsset, [NotNullWhen(true)] out Bshdat? shader)
	{
		string shaderPath;

		shader = null;

		if (material.Extras.TryGetProperty(GltfProperties.ShaderPath, out string? customShader))
			shaderPath = customShader;
		else
			shaderPath = DefaultShader;

		if (!AssetResolver.TryGetExistingAsset(shaderPath, out shaderAsset))
		{
			if (shaderPath == DefaultShader)
			{
				Warn($"Cannot find default shader \"{DefaultShader}\"! Materials cannot be imported.");
				return false;
			}

			Warn($"Material \"{material.Name}\" referenced invalid shader \"{shaderAsset}\". This material will use the default shader.");
			shaderPath = DefaultShader;
		}

		// Check again in case the previous unresolved asset location was custom. If this fails, it means the default shader also cannot be found.
		if (!AssetResolver.TryGetExistingAsset(shaderPath, out shaderAsset))
		{
			Warn($"Cannot find default shader \"{DefaultShader}\"! Materials cannot be imported.");
			return false;
		}

		try
		{
			shader = shaderAsset.ReadAs<Bshdat>();
			return true;
		}
		catch (Exception ex)
		{
			Warn($"Error loading shader \"{shaderPath}\": {ex}");
			return false;
		}
	}

	private bool TryGetPrototypeMaterialForShader(GameAsset shaderAsset, [NotNullWhen(true)] out GameAsset? prototypeMaterialAsset)
	{
		prototypeMaterialAsset = null;

		if (!ShaderNameRegex.IsMatch(shaderAsset.RelativePath, out var match))
			return false;

		var shaderName = match.Groups["Name"].Value;
		var materialName = $"system/engine/surfaces/{shaderName}.bsmat";

		return AssetResolver.TryGetExistingAsset(materialName, out prototypeMaterialAsset);
	}

	private void BindBaseColorTexture(SGMaterial material, Texture mainTexture, Texture? subTexture, Sampler sampler)
	{
		var combinedImage = GetCombinedImage(material, mainTexture, subTexture, TextureConverter.CombineBaseColorAndEmissive);

		if (combinedImage is null)
			return;

		BindTextureImageToSampler(mainTexture, sampler, combinedImage, useSrgb: true);
	}

	private void BindAttributesTexture(SGMaterial material, Texture mainTexture, Texture? subTexture, Sampler sampler)
	{
		var combinedImage = GetCombinedImage(material, mainTexture, subTexture, TextureConverter.CombineMetallicRoughnessAndOcclusion);

		if (combinedImage is null)
			return;

		BindTextureImageToSampler(mainTexture, sampler, combinedImage, useSrgb: false);
	}

	private void BindNormalsTexture(SGMaterial material, Texture mainTexture, Sampler sampler)
	{
		var convertedImage = GetConvertedImage(material, mainTexture, TextureConverter.ConvertNormalMapToDread);

		if (convertedImage is null)
			return;

		BindTextureImageToSampler(mainTexture, sampler, convertedImage, useSrgb: false, isNormalMap: true);
	}

	private void BindTextureImageToSampler(Texture texture, Sampler sampler, MagickImage image, bool useSrgb, bool isNormalMap = false)
	{
		// Get/encode the texture as BCTEX
		EncodeTextureImage(texture.GetName() ?? GetUnknownTextureName(), image, useSrgb, isNormalMap, out var bctexPath);

		sampler.TexturePath = bctexPath;

		// Assign glTF sampler settings to BSMAT sampler
		ApplySamplerSettings(sampler, texture.Sampler);
	}

	private static void ApplySamplerSettings(Sampler bsmatSampler, TextureSampler gltfSampler)
	{
		bsmatSampler.TilingModeU = gltfSampler.WrapS.ToTilingMode();
		bsmatSampler.TilingModeV = gltfSampler.WrapT.ToTilingMode();
		bsmatSampler.MagnificationFilter = gltfSampler.MagFilter.ToFilterMode();
		bsmatSampler.MinificationFilter = gltfSampler.MinFilter.ToFilterMode();
		bsmatSampler.MipmapFilter = gltfSampler.MinFilter.ToFilterMode();
	}

	private MagickImage? GetCombinedImage(SGMaterial material, Texture mainTexture, Texture? subTexture, Func<MagickImage, MagickImage, MagickImage> combineImages)
	{
		if (CombinedTextureCache.TryGetValue(( mainTexture, subTexture ), out var combinedImage))
			return combinedImage;

		if (!DecodedImages.TryGetValue(mainTexture.PrimaryImage.Name, out var mainImage))
		{
			Warn($"Skipping texture \"{mainTexture.GetName()}\" for material \"{material.Name}\" because the texture image could not be found or decoded");
			return null;
		}

		// Different textures can use the same image, such as in the case of AO and PBR, where the Red channel of the image is AO
		// and the Green/Blue channels are PBR Metallic/Roughness. We don't need to combine them in this case.
		if (subTexture != null && !ReferenceEquals(mainTexture.PrimaryImage, subTexture.PrimaryImage))
		{
			if (DecodedImages.TryGetValue(subTexture.PrimaryImage.Name, out var subImage))
			{
				// Combine the textures
				mainImage = combineImages(mainImage, subImage);
				Disposables.Add(mainImage);
			}
			else
			{
				Warn($"Skipping secondary texture \"{subTexture.GetName()}\" for material \"{material.Name}\" because the texture image could not be found or decoded");
			}
		}

		CombinedTextureCache[( mainTexture, subTexture )] = mainImage;
		return mainImage;
	}

	private MagickImage? GetConvertedImage(SGMaterial material, Texture mainTexture, Func<MagickImage, MagickImage> convertSingleTexture)
	{
		if (ConvertedTextureCache.TryGetValue(mainTexture, out var convertedImage))
			return convertedImage;

		if (!DecodedImages.TryGetValue(mainTexture.PrimaryImage.Name, out var mainImage))
		{
			Warn($"Skipping texture \"{mainTexture.GetName()}\" for material \"{material.Name}\" because the texture image could not be found or decoded");
			return null;
		}

		// Convert the main texture
		mainImage = convertSingleTexture(mainImage);
		Disposables.Add(mainImage);
		ConvertedTextureCache[mainTexture] = mainImage;
		return mainImage;
	}

	private void EncodeTextureImage(string name, MagickImage image, bool useSrgb, bool isNormalMap, out string texturePath)
	{
		texturePath = $"{CurrentRomFsSubfolder}/models/textures/{name}.bctex";

		if (EncodedTextureCache.ContainsKey(image))
			return;

		CompressionFormat compressionFormat;
		string channelMapping;
		MseTextureKind textureKind;

		if (isNormalMap)
		{
			compressionFormat = CompressionFormat.Bc5;
			channelMapping = "RGB"; // Encoder only supports 3-channel arrays, even though it ignores the B channel
			textureKind = MseTextureKind.TwoChannelCompressed;
		}
		else
		{
			compressionFormat = image.ChannelCount switch {
				4 => CompressionFormat.Bc3,
				_ => CompressionFormat.Bc1,
			};
			channelMapping = image.ChannelCount switch {
				4 => "RGBA",
				_ => "RGB",
			};
			textureKind = image.ChannelCount switch {
				4 => MseTextureKind.Rgba,
				_ => MseTextureKind.OpaqueRgb,
			};
		}

		var newTexture = TegraTexture.FromImage(image, channelMapping, compressionFormat, TextureEncodingOptions);
		var bctex = new Bctex {
			TextureName = name,
			Width = image.Width,
			Height = image.Height,
			MipCount = newTexture.Info.MipCount,
			EncodingType = MseTextureEncoding.Dds,
			IsSrgb = useSrgb,
			TextureKind = textureKind,
			TextureUsage = TextureUsage.Normal,
			Textures = { newTexture },
		};

		// Get an asset instance for the BCTEX that we can use for writing it out later
		var assetPath = $"textures/{texturePath}";
		var bctexAsset = AssetResolver.GetAsset(assetPath, assetIdOverride: texturePath);

		// Add the BCTEX to the current result and store it in the cache
		CurrentResult!.Textures[name] = ( bctex, bctexAsset );
		EncodedTextureCache[image] = bctex;
	}

	private string GetUnknownTextureName()
	{
		var thisId = this.nextUnknownTextureId++;

		return $"texture{thisId:D3}";
	}

	#endregion

	[OverloadResolutionPriority(1)]
	private void Warn(FormattableString message)
		=> Warning?.Invoke(message.ToString());

	private void Warn(string message)
		=> Warning?.Invoke(message);

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

	#region Helper Types

	[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
	private record struct NodeWithMesh(string MeshName, Node Node, SGMesh Mesh);

	#endregion

	#region Regular Expressions

	[GeneratedRegex(@"^(?<Name>.*?)(?:\.(?<Index>\d+))?$")]
	private static partial Regex GetMeshNameRegex();

	[GeneratedRegex(@"^system/shd/(?<Name>.+)\.bshdat$")]
	private static partial Regex GetShaderNameRegex();

	private static readonly Regex MeshNameRegex   = GetMeshNameRegex();
	private static readonly Regex ShaderNameRegex = GetShaderNameRegex();

	#endregion
}