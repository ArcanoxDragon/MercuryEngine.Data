using System.Numerics;
using System.Runtime.Intrinsics;
using ImageMagick;
using MercuryEngine.Data.Converters.Utility;
using MercuryEngine.Data.TegraTextureLib.Utility;

namespace MercuryEngine.Data.Converters.Bcmdl;

internal static class TextureConverter
{
	private delegate void ImageRenderFunction(in ReadOnlyRgba source, in Rgba dest);

	private delegate void ImageCombineFunction(in ReadOnlyRgba main, in ReadOnlyRgba sub, in Rgba dest);

	#region Base Color and Emissive

	/// <summary>
	/// Separates an RGBA texture into an RGB base color texture, and an RGB emissive texture using the A
	/// component of the source texture to control the emissive amount.
	/// </summary>
	public static (MagickImage, MagickImage?) SeparateBaseColorAndEmissive(MagickImage inputTexture)
	{
		return SeparateTexture(
			inputTexture,
			(in source, in dest) => {
				dest.R = source.R;
				dest.G = source.G;
				dest.B = source.B;
			},
			(in source, in dest) => {
				dest.R = source.R * source.A;
				dest.G = source.G * source.A;
				dest.B = source.B * source.A;
			});
	}

	/// <summary>
	/// Combines an RGB base color texture with an RGB emissive texture to produce an RGBA texture
	/// where the Alpha channel represents the amount of emissivity. The emissive color is always
	/// determined by the base color.
	/// </summary>
	public static MagickImage CombineBaseColorAndEmissive(MagickImage baseColor, MagickImage emissive)
	{
		return CombineTextures(baseColor, emissive, (in main, in sub, in dest) => {
			var emissiveAmount = sub.Luminosity;

			dest.R = main.R;
			dest.G = main.G;
			dest.B = main.B;
			dest.A = emissiveAmount;
		});
	}

	/// <summary>
	/// For RGB textures with associated emissivity map, the shader will expect the alpha to be
	/// fully opaque (or to be used as actual alpha).
	/// </summary>
	public static MagickImage FillInEmptyEmissive(MagickImage baseColor)
	{
		if (baseColor.ChannelCount == 4)
			// Already has an alpha channel, use as-is
			return baseColor;

		return ConvertTexture(baseColor, (in source, in dest) => {
			dest.R = source.R;
			dest.G = source.G;
			dest.B = source.B;
			dest.A = 1f;
		});
	}

	#endregion

	#region Metallic/Roughness and AO

	/// <summary>
	/// Separates an RGBA texture into an RGB metallic/roughness texture and an RGB occlusion texture.
	/// The G and B channels of the source texture will be preserved to the metallic/roughness texture,
	/// except that the G channel will be clamped to be at least 0.05f. The A channel of the source
	/// texture will be transferred to the R channel of the occlusion texture.
	/// </summary>
	public static (MagickImage, MagickImage?) SeparateMetallicRoughnessAndOcclusion(MagickImage inputTexture)
	{
		return SeparateTexture(
			inputTexture,
			(in source, in dest) => {
				dest.R = 0.0f;
				dest.G = Math.Max(0.05f, source.G);
				dest.B = source.B;
			},
			(in source, in dest) => {
				dest.R = source.A;
				dest.G = 0.0f;
				dest.B = 0.0f;
			});
	}

	/// <summary>
	/// Combines an RGB metallic/roughness texture and a texture where the Red channel represents ambient
	/// occlusion into a single RGBA texture where the Alpha channel represents ambient occlusion.
	/// </summary>
	public static MagickImage CombineMetallicRoughnessAndOcclusion(MagickImage baseColor, MagickImage emissive)
	{
		return CombineTextures(baseColor, emissive, (in main, in sub, in dest) => {
			dest.R = main.R;
			dest.G = main.G;
			dest.B = main.B;
			dest.A = sub.R;
		});
	}

	/// <summary>
	/// Converts an RGB occlusion/metallic/roughness texture from glTF into the format Dread expects,
	/// which is an unused R channel and occlusion information stored in A.
	/// </summary>
	public static MagickImage ConvertMetallicRoughnessOcclusionToDread(MagickImage inputTexture)
	{
		return ConvertTexture(inputTexture, (in source, in dest) => {
			dest.R = 0f;
			dest.G = source.G;
			dest.B = source.B;
			dest.A = source.R;
		});
	}

	#endregion

	#region Normal Maps

	/// <summary>
	/// Converts a two-channel normal map (in the style of Dread) to a 3-channel normal map by
	/// using the X and Y coordinates from the source map and inferring a Z coordinate that would
	/// result in a normalized 3-dimensional normal vector.
	/// </summary>
	public static MagickImage ConvertNormalMapFromDread(MagickImage inputTexture)
	{
		return ConvertTexture(inputTexture, (in source, in dest) => {
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

	/// <summary>
	/// Converts a three-channel normal map to a two-channel map (like Dread expects) by simply
	/// taking the X and Y coordinates from the source map and normalizing them as a 2-dimensional
	/// vector.
	/// </summary>
	public static MagickImage ConvertNormalMapToDread(MagickImage inputTexture)
	{
		return ConvertTexture(inputTexture, (in source, in dest) => {
			var normalXY = new Vector2(source.R, source.G);

			normalXY = Vector2.Normalize(normalXY);

			dest.R = normalXY.X;
			dest.G = normalXY.Y;
			dest.B = 0f;
		});
	}

	#endregion

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

	private static MagickImage CombineTextures(MagickImage mainTexture, MagickImage subTexture, ImageCombineFunction combineFunction)
	{
		const int DestPixelSize = 4;
		const float PreScale = 1f / 65535f;
		const float PostScale = 65535f;

		if (mainTexture.Width != subTexture.Width || mainTexture.Height != subTexture.Height)
			throw new ArgumentException("Both input textures must have the same dimensions");

		using var mainPixels = mainTexture.GetPixels();
		var mainPixelsData = mainPixels.GetArea(0, 0, mainTexture.Width, mainTexture.Height)!;
		var mainPixelsSpan = mainPixelsData.AsSpan();
		var mainPixelSize = (int) mainTexture.ChannelCount;

		using var subPixels = subTexture.GetPixels();
		var subPixelsData = subPixels.GetArea(0, 0, subTexture.Width, subTexture.Height)!;
		var subPixelsSpan = subPixelsData.AsSpan();
		var subPixelSize = (int) subTexture.ChannelCount;
		var subPixelStart = 0;

		var destPixelMemory = MemoryOwner<float>.Rent((int) ( mainTexture.Width * mainTexture.Height * DestPixelSize ));
		var destPixelsSpan = destPixelMemory.Span;
		var destPixelStart = 0;

		// Pre-scale the main pixels so they're in the range [0, 1]
		ScaleValues(mainPixelsSpan, PreScale);
		ScaleValues(subPixelsSpan, PreScale);

		for (var mainPixelStart = 0; mainPixelStart < mainPixelsSpan.Length; mainPixelStart += mainPixelSize)
		{
			var mainPixelEnd = mainPixelStart + mainPixelSize;
			var mainPixel = new ReadOnlyRgba(mainPixelsSpan[mainPixelStart..mainPixelEnd]);

			var subPixelEnd = subPixelStart + subPixelSize;
			var subPixel = new ReadOnlyRgba(subPixelsSpan[subPixelStart..subPixelEnd]);

			var destPixelEnd = destPixelStart + DestPixelSize;
			var destPixel = new Rgba(destPixelsSpan[destPixelStart..destPixelEnd]);

			combineFunction(in mainPixel, in subPixel, in destPixel);
			subPixelStart += subPixelSize;
			destPixelStart += DestPixelSize;
		}

		// Post-scale the dest pixels so they're in the range [0, 65535]
		ScaleValues(destPixelsSpan, PostScale);

		var outputTexture = new MagickImage();
		var readSettings = new PixelReadSettings(mainTexture.Width, mainTexture.Height, StorageType.Quantum, PixelMapping.RGBA) {
			ReadSettings = {
				ColorSpace = ColorSpace.Undefined,
			},
		};

		outputTexture.ReadPixels(destPixelsSpan, readSettings);

		return outputTexture;
	}

	private static MagickImage ConvertTexture(MagickImage inputTexture, ImageRenderFunction renderFunction)
	{
		const int DestPixelSize = 4;
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
			var destPixel = new Rgba(destPixelsSpan[destPixelStart..destPixelEnd]);

			renderFunction(in sourcePixel, in destPixel);
			destPixelStart += DestPixelSize;
		}

		// Post-scale the dest pixels so they're in the range [0, 65535]
		ScaleValues(destPixelsSpan, PostScale);

		var outputTexture = new MagickImage();
		var readSettings = new PixelReadSettings(inputTexture.Width, inputTexture.Height, StorageType.Quantum, PixelMapping.RGBA) {
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
				var vector = Vector512.Create(thisSpan);

				vector *= scale;
				vector.CopyTo(thisSpan);
			}
		}
		else if (values.Length % 8 == 0 && Vector256.IsHardwareAccelerated)
		{
			for (var i = 0; i < values.Length; i += 8)
			{
				var thisSpan = values[i..( i + 8 )];
				var vector = Vector256.Create(thisSpan);

				vector *= scale;
				vector.CopyTo(thisSpan);
			}
		}
		else if (values.Length % 4 == 0 && Vector128.IsHardwareAccelerated)
		{
			for (var i = 0; i < values.Length; i += 4)
			{
				var thisSpan = values[i..( i + 4 )];
				var vector = Vector128.Create(thisSpan);

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

		public float Luminosity => ColorUtility.RgbToHsl(R, G, B).L;
	}

	private readonly ref struct Rgba(Span<float> channels)
	{
		private readonly Span<float> channels = channels;

		public ref float R => ref this.channels[0];
		public ref float G => ref this.channels[1];
		public ref float B => ref this.channels[2];
		public ref float A => ref this.channels[3];

		public float Luminosity => ColorUtility.RgbToHsl(R, G, B).L;
	}
}