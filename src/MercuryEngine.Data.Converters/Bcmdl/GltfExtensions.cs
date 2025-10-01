using SharpGLTF.Schema2;

namespace MercuryEngine.Data.Converters.Bcmdl;

internal static class GltfExtensions
{
	public static string? GetName(this Texture texture)
		=> texture.Name ?? texture.PrimaryImage.Name;
}