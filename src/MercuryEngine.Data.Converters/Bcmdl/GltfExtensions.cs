using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace MercuryEngine.Data.Converters.Bcmdl;

internal static class GltfExtensions
{
	public static string? GetName(this Texture texture)
		=> texture.Name ?? texture.PrimaryImage.Name;

	public static bool IsColorChannel(this KnownChannel channel)
#pragma warning disable CS0618 // Type or member is obsolete
		=> channel switch {
			KnownChannel.Occlusion                => true,
			KnownChannel.Emissive                 => true,
			KnownChannel.BaseColor                => true,
			KnownChannel.Diffuse                  => true,
			KnownChannel.ClearCoat                => true,
			KnownChannel.SheenColor               => true,
			KnownChannel.SpecularColor            => true,
			KnownChannel.DiffuseTransmissionColor => true,

			_ => false,
		};
#pragma warning restore CS0618 // Type or member is obsolete
}