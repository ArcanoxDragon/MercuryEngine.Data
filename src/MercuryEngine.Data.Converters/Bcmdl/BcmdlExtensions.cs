using MercuryEngine.Data.Types.Bcmdl;
using MercuryEngine.Data.Types.Bsmat;
using SharpGLTF.Schema2;
using Material = MercuryEngine.Data.Types.Bcmdl.Material;
using Mesh = MercuryEngine.Data.Types.Bcmdl.Mesh;

namespace MercuryEngine.Data.Converters.Bcmdl;

public static class BcmdlExtensions
{
	public static bool IsSkinned(this Mesh mesh)
	{
		if (mesh.VertexBuffer is not { } vertexBuffer)
			return false;

		var hasJointIndices = vertexBuffer.VertexInfoSlots.Any(s => s.Type == VertexInfoType.JointIndex);
		var hasJointWeights = vertexBuffer.VertexInfoSlots.Any(s => s.Type == VertexInfoType.JointWeight);

		return hasJointIndices && hasJointWeights;
	}

	public static BcmdlGeometryType GetGeometryType(this VertexBuffer vertexBuffer)
	{
		var hasPosition = vertexBuffer.VertexInfoSlots.Any(s => s.Type == VertexInfoType.Position);
		var hasNormal = vertexBuffer.VertexInfoSlots.Any(s => s.Type == VertexInfoType.Normal);
		var hasTangent = vertexBuffer.VertexInfoSlots.Any(s => s.Type == VertexInfoType.Tangent);

		return ( hasPosition, hasNormal, hasTangent ) switch {
			(true, true, true)   => BcmdlGeometryType.PositionNormalTangent,
			(true, true, false)  => BcmdlGeometryType.PositionNormal,
			(true, false, false) => BcmdlGeometryType.Position,
			_                    => BcmdlGeometryType.Unknown,
		};
	}

	public static BcmdlMaterialType GetMaterialType(this Material material)
	{
		if (material is { Tex3Name: not null, Tex3Parameters: not null })
			return BcmdlMaterialType.Color1Texture3;
		if (material is { Tex2Name: not null, Tex2Parameters: not null })
			return BcmdlMaterialType.Color1Texture2;
		if (material is { Tex1Name: not null, Tex1Parameters: not null })
			return BcmdlMaterialType.Color1Texture1;

		return BcmdlMaterialType.Color1;
	}

	#region Sampler Enums

	public static TextureWrapMode ToTextureWrapMode(this TilingMode tilingMode)
		=> tilingMode switch {
			TilingMode.ClampToColor   => TextureWrapMode.CLAMP_TO_EDGE,
			TilingMode.Repeat         => TextureWrapMode.REPEAT,
			TilingMode.MirroredRepeat => TextureWrapMode.MIRRORED_REPEAT,
			_                         => TextureWrapMode.CLAMP_TO_EDGE,
		};

	public static TilingMode ToTilingMode(this TextureWrapMode textureWrapMode)
		=> textureWrapMode switch {
			TextureWrapMode.REPEAT          => TilingMode.Repeat,
			TextureWrapMode.MIRRORED_REPEAT => TilingMode.MirroredRepeat,
			_                               => TilingMode.Clamp,
		};

	public static TextureMipMapFilter ToTextureMipMapFilter(this FilterMode magnificationFilter)
		=> magnificationFilter switch {
			FilterMode.Nearest           => TextureMipMapFilter.NEAREST,
			FilterMode.Linear            => TextureMipMapFilter.LINEAR,
			FilterMode.NearestMipNearest => TextureMipMapFilter.NEAREST_MIPMAP_NEAREST,
			FilterMode.NearestMipLinear  => TextureMipMapFilter.NEAREST_MIPMAP_LINEAR,
			FilterMode.LinearMipNearest  => TextureMipMapFilter.LINEAR_MIPMAP_NEAREST,
			FilterMode.LinearMipLinear   => TextureMipMapFilter.LINEAR_MIPMAP_LINEAR,
			_                            => TextureMipMapFilter.DEFAULT,
		};

	public static FilterMode ToFilterMode(this TextureMipMapFilter textureMipMapFilter)
		=> textureMipMapFilter switch {
			TextureMipMapFilter.LINEAR                 => FilterMode.Linear,
			TextureMipMapFilter.NEAREST                => FilterMode.Nearest,
			TextureMipMapFilter.NEAREST_MIPMAP_NEAREST => FilterMode.NearestMipNearest,
			TextureMipMapFilter.LINEAR_MIPMAP_NEAREST  => FilterMode.LinearMipNearest,
			TextureMipMapFilter.LINEAR_MIPMAP_LINEAR   => FilterMode.LinearMipLinear,
			_                                          => FilterMode.NearestMipLinear,
		};

	public static TextureInterpolationFilter ToTextureInterpolationFilter(this FilterMode minifactionFilter)
		=> minifactionFilter switch {
			FilterMode.Nearest => TextureInterpolationFilter.NEAREST,
			FilterMode.Linear  => TextureInterpolationFilter.LINEAR,
			_                  => TextureInterpolationFilter.DEFAULT,
		};

	public static FilterMode ToFilterMode(this TextureInterpolationFilter textureInterpolationFilter)
		=> textureInterpolationFilter switch {
			TextureInterpolationFilter.LINEAR  => FilterMode.Linear,
			TextureInterpolationFilter.NEAREST => FilterMode.Nearest,
			_                                  => FilterMode.NearestMipLinear,
		};

	#endregion
}