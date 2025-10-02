namespace MercuryEngine.Data.Converters.Bcmdl;

public static class GltfProperties
{
	/// <summary>
	/// Used to designate a custom shader (BSHDAT) path for glTF materials.
	/// </summary>
	public const string ShaderPath = nameof(ShaderPath);

	/// <summary>
	/// Used to designate a custom texture (BCTEX) path for glTF textures.
	/// </summary>
	public const string TexturePath = nameof(TexturePath);

	/// <summary>
	/// Used to designate a custom texture (BCTEX) path for material samplers.
	/// </summary>
	/// <param name="samplerName">The name of the sampler this property is for.</param>
	public static string SamplerTexturePath(string samplerName) => $"{samplerName}_TexturePath";
}