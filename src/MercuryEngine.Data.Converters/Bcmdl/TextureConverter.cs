using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using ImageMagick;
using MercuryEngine.Data.TegraTextureLib.Utility;

namespace MercuryEngine.Data.Converters.Bcmdl;

internal static class TextureConverter
{
	private delegate void ImageRenderFunction(in ReadOnlyRgba source, in Rgb dest);

	/// <summary>
	/// Separates an RGBA texture into an RGB base color texture, and an RGB emissive texture using the A
	/// component of the source texture to control the emissive amount.
	/// </summary>
	public static (MagickImage, MagickImage?) SeparateBaseColorAndEmissive(MagickImage inputTexture)
	{
		// Remove alpha channel of base color texture
		const string MainSamplingCode =
			"result = half4(sample.rgb, 1.0);";

		// Premultiply emissive color using alpha channel of base color texture
		const string SubSamplingCode =
			"result = half4(sample.rgb * sample.a, 1.0);";

		return SeparateTexture(
			inputTexture,
			(in ReadOnlyRgba source, in Rgb dest) => {
				dest.R = source.R;
				dest.G = source.G;
				dest.B = source.B;
			},
			(in ReadOnlyRgba source, in Rgb dest) => {
				dest.R = source.R * source.A;
				dest.G = source.G * source.A;
				dest.B = source.B * source.A;
			});
	}

	/// <summary>
	/// Separates an RGBA texture into an RGB metallic/roughness texture and an RGB occlusion texture.
	/// The G and B channels of the source texture will be preserved to the metallic/roughness texture,
	/// except that the G channel will be clamped to be at least 0.05f. The A channel of the source
	/// texture will be transferred to the R channel of the occlusion texture.
	/// </summary>
	public static (MagickImage, MagickImage?) SeparateMetallicRoughnessAndOcclusion(MagickImage inputTexture)
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

		return SeparateTexture(
			inputTexture,
			(in ReadOnlyRgba source, in Rgb dest) => {
				dest.R = 0.0f;
				dest.G = Math.Max(0.05f, source.G);
				dest.B = source.B;
			},
			(in ReadOnlyRgba source, in Rgb dest) => {
				dest.R = source.A;
				dest.G = 0.0f;
				dest.B = 0.0f;
			});
	}

	public static MagickImage ConvertNormalMap(MagickImage inputTexture)
	{
		// Ensure B channel is fully saturated (Dread uses 2D normal maps, but most 3D software expects 3D maps)
		const string SamplingCode =
			"""
			half2 normalXY = sample.rg * 2.0 - 1.0;
			half normalZ = sqrt(saturate(1.0 - normalXY.x * normalXY.x + normalXY.y * normalXY.y));
			half3 normal = half3(normalXY, normalZ);
			result = half4((normal + 1.0) / 2.0, 1.0);
			""";

		return ConvertTexture(inputTexture, (in ReadOnlyRgba source, in Rgb dest) => {
			var normalXY = ( new Vector2(source.R, source.G) * 2.0f ) - Vector2.One;
			var normalZ = (float) Math.Sqrt(Math.Clamp(1.0f - ( normalXY.X * normalXY.X ) + ( normalXY.Y * normalXY.Y ), 0f, 1f));
			var normal = new Vector3(normalXY, normalZ);

			normal += Vector3.One;
			normal /= 2.0f;

			dest.R = normal.X;
			dest.G = normal.Y;
			dest.B = normal.Z;
		});
	}

	private static (MagickImage, MagickImage?) SeparateTexture(
		MagickImage inputTexture,
		ImageRenderFunction mainRenderFunction,
		ImageRenderFunction subRenderFunction)
	{
		if (inputTexture.ChannelCount != 4)
			return ( inputTexture, null );

		var mainTexture = ConvertTexture(inputTexture, mainRenderFunction);
		var subTexture = ConvertTexture(inputTexture, subRenderFunction);

		return ( mainTexture, subTexture );
	}

	private static MagickImage ConvertTexture(MagickImage inputTexture, ImageRenderFunction renderFunction)
	{
		const int DestPixelSize = 3;
		const float PreScale = 1f / 65535f;
		const float PostScale = 65535f;

		using var sourcePixels = inputTexture.GetPixels();
		var sourcePixelsData = sourcePixels.GetArea(0, 0, inputTexture.Width, inputTexture.Height)!;
		var sourcePixelsSpan = sourcePixelsData.AsSpan();
		var sourcePixelSize = (int) inputTexture.ChannelCount;
		var destPixelMemory = MemoryOwner<float>.Rent((int) ( inputTexture.Width * inputTexture.Height * DestPixelSize ));
		var destPixelsSpan = destPixelMemory.Span;
		var destPixelStart = 0;

		// Pre-scale the source pixels so they're in the range [0, 1]
		ScaleValues(sourcePixelsSpan, PreScale);

		for (var sourcePixelStart = 0; sourcePixelStart < sourcePixelsSpan.Length; sourcePixelStart += sourcePixelSize)
		{
			var sourcePixelEnd = sourcePixelStart + sourcePixelSize;
			var sourcePixel = new ReadOnlyRgba(sourcePixelsSpan[sourcePixelStart..sourcePixelEnd]);
			var destPixelEnd = destPixelStart + DestPixelSize;
			var destPixel = new Rgb(destPixelsSpan[destPixelStart..destPixelEnd]);

			renderFunction(in sourcePixel, in destPixel);
			destPixelStart += DestPixelSize;
		}

		// Post-scale the dest pixels so they're in the range [0, 65535]
		ScaleValues(destPixelsSpan, PostScale);

		var outputTexture = new MagickImage();
		var readSettings = new PixelReadSettings(inputTexture.Width, inputTexture.Height, StorageType.Quantum, PixelMapping.RGB) {
			ReadSettings = {
				ColorSpace = ColorSpace.Undefined,
			},
		};

		outputTexture.ReadPixels(destPixelsSpan, readSettings);

		return outputTexture;
	}

	private static void ScaleValues(Span<float> values, float scale)
	{
		if (values.Length % 16 == 0 && Vector256.IsHardwareAccelerated)
		{
			for (var i = 0; i < values.Length; i += 16)
			{
				var thisSpan = values[i..( i + 16 )];
				var vector = Vector512.Create((ReadOnlySpan<float>) thisSpan);

				vector *= scale;
				vector.CopyTo(thisSpan);
			}
		}
		else if (values.Length % 8 == 0 && Vector256.IsHardwareAccelerated)
		{
			for (var i = 0; i < values.Length; i += 8)
			{
				var thisSpan = values[i..( i + 8 )];
				var vector = Vector256.Create((ReadOnlySpan<float>) thisSpan);

				vector *= scale;
				vector.CopyTo(thisSpan);
			}
		}
		else if (values.Length % 4 == 0 && Vector128.IsHardwareAccelerated)
		{
			for (var i = 0; i < values.Length; i += 4)
			{
				var thisSpan = values[i..( i + 4 )];
				var vector = Vector128.Create((ReadOnlySpan<float>) thisSpan);

				vector *= scale;
				vector.CopyTo(thisSpan);
			}
		}
		else
		{
			for (var i = 0; i < values.Length; i++)
				values[i] *= scale;
		}
	}

	private readonly ref struct ReadOnlyRgba(ReadOnlySpan<float> channels)
	{
		private readonly ReadOnlySpan<float> channels = channels;

		public ref readonly float R => ref this.channels[0];
		public ref readonly float G => ref this.channels[1];
		public ref readonly float B => ref this.channels[2];
		public ref readonly float A => ref this.channels[3];
	}

	private readonly ref struct Rgb(Span<float> channels)
	{
		private readonly Span<float> channels = channels;

		public ref float R => ref this.channels[0];
		public ref float G => ref this.channels[1];
		public ref float B => ref this.channels[2];
	}
}