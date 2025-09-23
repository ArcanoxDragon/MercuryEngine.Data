using System.Numerics;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Formats;
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
using Mesh = MercuryEngine.Data.Types.Bcmdl.Mesh;

namespace MercuryEngine.Data.Converters.Bcmdl;

public class GltfExporter(IMaterialResolver? materialResolver = null)
{
	private const string TextureNameBaseColor  = "texBaseColor";
	private const string TextureNameNormals    = "texNormals";
	private const string TextureNameAttributes = "texAttributes";

	public IMaterialResolver? MaterialResolver { get; } = materialResolver;

	public void ExportGltf(Formats.Bcmdl bcmdl, string targetFilePath, bool binary = true, string? sceneName = null)
	{
		var scene = new SceneBuilder(sceneName);

		foreach (var mesh in bcmdl.Meshes)
		{
			if (mesh is null)
				continue;

			var meshBuilder = BuildMesh(mesh);
			var nodeBuilder = new NodeBuilder(mesh.Id?.Name);
			var localMatrix = Matrix4x4.Identity;

			if (mesh.Submesh is { } submesh)
				// TODO: Not sure whether or not (or how) to consume the submesh's transformation matrix
				//  (submeshes store both a matrix AND a separate translation vector)
				localMatrix.Translation = submesh.Translation;

			// Scale translation portion of matrix from cm to m before applying it
			localMatrix.Translation = ScalePosition(localMatrix.Translation);
			nodeBuilder.LocalMatrix = localMatrix;
			scene.AddRigidMesh(meshBuilder, nodeBuilder).WithName(mesh.Id?.Name);
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

	private IMeshBuilder<MaterialBuilder>? BuildMesh(Mesh mesh)
	{
		var meshBuilder = CreateMeshBuilder(mesh, mesh.Id?.Name);

		if (meshBuilder is null)
			return null;

		var materialBuilder = new MaterialBuilder(mesh.Material?.Name);

		if (mesh.Material?.Path is { } materialPath && MaterialResolver?.LoadMaterial(materialPath) is { } material)
			AssignMaterials(materialBuilder, material);

		FillPrimitives(mesh, meshBuilder, materialBuilder);

		return meshBuilder;
	}

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

	private static IMeshBuilder<MaterialBuilder>? CreateMeshBuilder(Mesh mesh, string? name)
	{
		if (mesh is not { Material: { } material, Submesh.VertexBuffer: { } vertexBuffer })
			return null;

		var geometryType = vertexBuffer.GetGeometryType();
		var materialType = material.GetMaterialType();
		var isSkinned = mesh.IsSkinned();

		return ( geometryType, materialType, isSkinned ) switch {
			// Non-skinned
			//   Position-only vertices
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPosition, VertexColor1Texture1>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPosition, VertexColor1Texture2>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPosition, VertexColor1Texture3>(name),
			//   Position-Normal vertices
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3>(name),
			//   Position-Normal-Tangent vertices
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture1, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture1>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture2, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture2>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture3, false) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture3>(name),

			// Skinned
			//   Position-only vertices
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPosition, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPosition, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.Position, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPosition, VertexColor1Texture3, VertexJoints4>(name),
			//   Position-Normal vertices
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormal, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3, VertexJoints4>(name),
			//   Position-Normal-Tangent vertices
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture1, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture1, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture2, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture2, VertexJoints4>(name),
			(BcmdlGeometryType.PositionNormalTangent, BcmdlMaterialType.Color1Texture3, true) => new MeshBuilder<VertexPositionNormalTangent, VertexColor1Texture3, VertexJoints4>(name),

			_ => null,
		};
	}

	private static void FillPrimitives(Mesh mesh, IMeshBuilder<MaterialBuilder> meshBuilder, MaterialBuilder materialBuilder)
	{
		if (mesh is not { Submesh: { SubmeshInfos: { } submeshInfos, VertexBuffer: { } vertexBuffer, IndexBuffer: { } indexBuffer } })
			return;

		var vertices = vertexBuffer.GetVertices();
		var indices = indexBuffer.GetIndices();

		foreach (var submeshInfo in submeshInfos)
		{
			if (submeshInfo is null || submeshInfo.IndexCount % 3 != 0)
				// TODO: Need way of communicating warnings to consumer
				continue;

			var finalIndex = submeshInfo.IndexOffset + submeshInfo.IndexCount - 1;

			if (finalIndex >= indices.Length)
				// TODO: Need way of communicating warnings to consumer
				continue;

			var primitiveBuilder = meshBuilder.UsePrimitive(materialBuilder);

			for (var i = 0; i < indices.Length; i += 3)
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

	private static Vector3 ScalePosition(Vector3 rawPosition)
		// Scale cm to meters
		=> new(rawPosition.X / 100f, rawPosition.Y / 100f, rawPosition.Z / 100f);

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
}