using SkiaSharp;

namespace MercuryEngine.Data.Converters.Bcmdl;

internal static class TextureConverter
{
	/// <summary>
	/// Separates an RGBA texture into an RGB base color texture, and an RGB emissive texture using the A
	/// component of the source texture to control the emissive amount.
	/// </summary>
	public static (SKBitmap, SKBitmap?) SeparateBaseColorAndEmissive(SKBitmap inputTexture)
	{
		// Remove alpha channel of base color texture
		const string MainSamplingCode =
			"result = half4(sample.rgb, 1.0);";

		// Premultiply emissive color using alpha channel of base color texture
		const string SubSamplingCode =
			"result = half4(sample.rgb * sample.a, 1.0);";

		return SeparateTexture(inputTexture, MainSamplingCode, SubSamplingCode);
	}

	/// <summary>
	/// Separates an RGBA texture into an RGB metallic/roughness texture and an RGB occlusion texture.
	/// The G and B channels of the source texture will be preserved to the metallic/roughness texture,
	/// except that the G channel will be clamped to be at least 0.05f. The A channel of the source
	/// texture will be transferred to the R channel of the occlusion texture.
	/// </summary>
	public static (SKBitmap, SKBitmap?) SeparateMetallicRoughnessAndOcclusion(SKBitmap inputTexture)
	{
		// Clamp roughness to a minimum of 0.05, and keep metallic the same
		const string MainSamplingCode =
			"""
			float clampedRoughness = max(sample.g, 0.05);

			result = half4(0, clampedRoughness, sample.b, 1.0);
			""";

		// Move occlusion to its own texture as the R channel
		const string SubSamplingCode =
			"result = half4(sample.a, 0, 0, 1);";

		return SeparateTexture(inputTexture, MainSamplingCode, SubSamplingCode);
	}

	public static SKBitmap ConvertNormalMap(SKBitmap inputTexture)
	{
		// Ensure B channel is fully saturated (Dread uses 2D normal maps, but most 3D software expects 3D maps)
		const string SamplingCode =
			"""
			half2 normalXY = sample.rg * 2.0 - 1.0;
			half normalZ = sqrt(saturate(1.0 - normalXY.x * normalXY.x + normalXY.y * normalXY.y));
			half3 normal = half3(normalXY, normalZ);
			result = half4((normal + 1.0) / 2.0, 1.0);
			""";

		return ConvertTexture(inputTexture, SamplingCode);
	}

	private static (SKBitmap, SKBitmap?) SeparateTexture(
		SKBitmap inputTexture,
		string mainSamplingCode,
		string subSamplingCode)
	{
		if (inputTexture.ColorType is not (SKColorType.Rgba8888 or SKColorType.Bgra8888))
			return ( inputTexture, null );

		using var sourceImage = SKImage.FromBitmap(inputTexture);
		using var inputImageShader = sourceImage.ToRawShader();
		var mainTexture = new SKBitmap(inputTexture.Width, inputTexture.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
		var subTexture = new SKBitmap(inputTexture.Width, inputTexture.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);

		RenderUsingShader(inputImageShader, mainTexture, mainSamplingCode);
		RenderUsingShader(inputImageShader, subTexture, subSamplingCode);

		return ( mainTexture, subTexture );
	}

	private static SKBitmap ConvertTexture(SKBitmap inputTexture, string samplingCode)
	{
		using var sourceImage = SKImage.FromBitmap(inputTexture);
		using var inputImageShader = sourceImage.ToRawShader();
		var outputTexture = new SKBitmap(inputTexture.Width, inputTexture.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);

		RenderUsingShader(inputImageShader, outputTexture, samplingCode);

		return outputTexture;
	}

	private static void RenderUsingShader(SKShader inputImageShader, SKBitmap outputImage, string samplingCode)
	{
		var fullShaderSource =
			$$"""
			  uniform shader image;

			  half4 main(float2 coord) {
			    half4 sample = image.eval(coord);
			    half4 result = half4(0, 0, 0, 1);
			    {{samplingCode}}
			    return result;
			  }
			  """;
		using var effectBuilder = SKRuntimeEffect.BuildShader(fullShaderSource);

		effectBuilder.Children.Add("image", inputImageShader);

		using var shader = effectBuilder.Build();
		using var canvas = new SKCanvas(outputImage);
		using var paint = new SKPaint();

		paint.Shader = shader;
		canvas.DrawPaint(paint);
		canvas.Flush();
	}
}