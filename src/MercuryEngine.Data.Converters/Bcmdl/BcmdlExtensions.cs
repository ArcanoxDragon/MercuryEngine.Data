using MercuryEngine.Data.Types.Bcmdl;

namespace MercuryEngine.Data.Converters.Bcmdl;

public static class BcmdlExtensions
{
	public static bool IsSkinned(this Mesh meshNode)
	{
		if (meshNode.VertexBuffer is not { } vertexBuffer)
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
}