using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.TegraTextureLib;

public enum TextureImageFormat
{
	R8Unorm,
	R8Snorm,
	R8Uint,
	R8Sint,
	R16Float,
	R16Unorm,
	R16Snorm,
	R16Uint,
	R16Sint,
	R32Float,
	R32Uint,
	R32Sint,
	R8G8Unorm,
	R8G8Snorm,
	R8G8Uint,
	R8G8Sint,
	R16G16Float,
	R16G16Unorm,
	R16G16Snorm,
	R16G16Uint,
	R16G16Sint,
	R32G32Float,
	R32G32Uint,
	R32G32Sint,
	R8G8B8Unorm,
	R8G8B8Snorm,
	R8G8B8Uint,
	R8G8B8Sint,
	R16G16B16Float,
	R16G16B16Unorm,
	R16G16B16Snorm,
	R16G16B16Uint,
	R16G16B16Sint,
	R32G32B32Float,
	R32G32B32Uint,
	R32G32B32Sint,
	R8G8B8A8Unorm,
	R8G8B8A8Snorm,
	R8G8B8A8Uint,
	R8G8B8A8Sint,
	R16G16B16A16Float,
	R16G16B16A16Unorm,
	R16G16B16A16Snorm,
	R16G16B16A16Uint,
	R16G16B16A16Sint,
	R32G32B32A32Float,
	R32G32B32A32Uint,
	R32G32B32A32Sint,
	S8Uint,
	D16Unorm,
	S8UintD24Unorm,
	D32Float,
	D24UnormS8Uint,
	D32FloatS8Uint,
	R8G8B8A8Srgb,
	R4G4Unorm,
	R4G4B4A4Unorm,
	R5G5B5X1Unorm,
	R5G5B5A1Unorm,
	R5G6B5Unorm,
	R10G10B10A2Unorm,
	R10G10B10A2Uint,
	R11G11B10Float,
	R9G9B9E5Float,
	Bc1RgbaUnorm,
	Bc2Unorm,
	Bc3Unorm,
	Bc1RgbaSrgb,
	Bc2Srgb,
	Bc3Srgb,
	Bc4Unorm,
	Bc4Snorm,
	Bc5Unorm,
	Bc5Snorm,
	Bc7Unorm,
	Bc7Srgb,
	Bc6HSfloat,
	Bc6HUfloat,
	Etc2RgbUnorm,
	Etc2RgbaUnorm,
	Etc2RgbPtaUnorm,
	Etc2RgbSrgb,
	Etc2RgbaSrgb,
	Etc2RgbPtaSrgb,
	R8Uscaled,
	R8Sscaled,
	R16Uscaled,
	R16Sscaled,
	R32Uscaled,
	R32Sscaled,
	R8G8Uscaled,
	R8G8Sscaled,
	R16G16Uscaled,
	R16G16Sscaled,
	R32G32Uscaled,
	R32G32Sscaled,
	R8G8B8Uscaled,
	R8G8B8Sscaled,
	R16G16B16Uscaled,
	R16G16B16Sscaled,
	R32G32B32Uscaled,
	R32G32B32Sscaled,
	R8G8B8A8Uscaled,
	R8G8B8A8Sscaled,
	R16G16B16A16Uscaled,
	R16G16B16A16Sscaled,
	R32G32B32A32Uscaled,
	R32G32B32A32Sscaled,
	R10G10B10A2Snorm,
	R10G10B10A2Sint,
	R10G10B10A2Uscaled,
	R10G10B10A2Sscaled,
	Astc4x4Unorm,
	Astc5x4Unorm,
	Astc5x5Unorm,
	Astc6x5Unorm,
	Astc6x6Unorm,
	Astc8x5Unorm,
	Astc8x6Unorm,
	Astc8x8Unorm,
	Astc10x5Unorm,
	Astc10x6Unorm,
	Astc10x8Unorm,
	Astc10x10Unorm,
	Astc12x10Unorm,
	Astc12x12Unorm,
	Astc4x4Srgb,
	Astc5x4Srgb,
	Astc5x5Srgb,
	Astc6x5Srgb,
	Astc6x6Srgb,
	Astc8x5Srgb,
	Astc8x6Srgb,
	Astc8x8Srgb,
	Astc10x5Srgb,
	Astc10x6Srgb,
	Astc10x8Srgb,
	Astc10x10Srgb,
	Astc12x10Srgb,
	Astc12x12Srgb,
	B5G6R5Unorm,
	B5G5R5A1Unorm,
	A1B5G5R5Unorm,
	B8G8R8A8Unorm,
	B8G8R8A8Srgb,
	B10G10R10A2Unorm,
	X8UintD24Unorm,
}

[SuppressMessage("ReSharper", "DuplicatedStatements")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "DuplicatedSwitchSectionBodies")]
public static class FormatExtensions
{
	/// <summary>
	/// The largest scalar size for a buffer format.
	/// </summary>
	public const int MaxBufferFormatScalarSize = 4;

	/// <summary>
	/// Gets the byte size for a single component of this format, or its packed size.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>Byte size for a single component, or packed size</returns>
	public static int GetScalarSize(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.R8Unorm:
			case TextureImageFormat.R8Snorm:
			case TextureImageFormat.R8Uint:
			case TextureImageFormat.R8Sint:
			case TextureImageFormat.R8G8Unorm:
			case TextureImageFormat.R8G8Snorm:
			case TextureImageFormat.R8G8Uint:
			case TextureImageFormat.R8G8Sint:
			case TextureImageFormat.R8G8B8Unorm:
			case TextureImageFormat.R8G8B8Snorm:
			case TextureImageFormat.R8G8B8Uint:
			case TextureImageFormat.R8G8B8Sint:
			case TextureImageFormat.R8G8B8A8Unorm:
			case TextureImageFormat.R8G8B8A8Snorm:
			case TextureImageFormat.R8G8B8A8Uint:
			case TextureImageFormat.R8G8B8A8Sint:
			case TextureImageFormat.R8G8B8A8Srgb:
			case TextureImageFormat.R4G4Unorm:
			case TextureImageFormat.R8Uscaled:
			case TextureImageFormat.R8Sscaled:
			case TextureImageFormat.R8G8Uscaled:
			case TextureImageFormat.R8G8Sscaled:
			case TextureImageFormat.R8G8B8Uscaled:
			case TextureImageFormat.R8G8B8Sscaled:
			case TextureImageFormat.R8G8B8A8Uscaled:
			case TextureImageFormat.R8G8B8A8Sscaled:
			case TextureImageFormat.B8G8R8A8Unorm:
			case TextureImageFormat.B8G8R8A8Srgb:
				return 1;

			case TextureImageFormat.R16Float:
			case TextureImageFormat.R16Unorm:
			case TextureImageFormat.R16Snorm:
			case TextureImageFormat.R16Uint:
			case TextureImageFormat.R16Sint:
			case TextureImageFormat.R16G16Float:
			case TextureImageFormat.R16G16Unorm:
			case TextureImageFormat.R16G16Snorm:
			case TextureImageFormat.R16G16Uint:
			case TextureImageFormat.R16G16Sint:
			case TextureImageFormat.R16G16B16Float:
			case TextureImageFormat.R16G16B16Unorm:
			case TextureImageFormat.R16G16B16Snorm:
			case TextureImageFormat.R16G16B16Uint:
			case TextureImageFormat.R16G16B16Sint:
			case TextureImageFormat.R16G16B16A16Float:
			case TextureImageFormat.R16G16B16A16Unorm:
			case TextureImageFormat.R16G16B16A16Snorm:
			case TextureImageFormat.R16G16B16A16Uint:
			case TextureImageFormat.R16G16B16A16Sint:
			case TextureImageFormat.R4G4B4A4Unorm:
			case TextureImageFormat.R5G5B5X1Unorm:
			case TextureImageFormat.R5G5B5A1Unorm:
			case TextureImageFormat.R5G6B5Unorm:
			case TextureImageFormat.R16Uscaled:
			case TextureImageFormat.R16Sscaled:
			case TextureImageFormat.R16G16Uscaled:
			case TextureImageFormat.R16G16Sscaled:
			case TextureImageFormat.R16G16B16Uscaled:
			case TextureImageFormat.R16G16B16Sscaled:
			case TextureImageFormat.R16G16B16A16Uscaled:
			case TextureImageFormat.R16G16B16A16Sscaled:
			case TextureImageFormat.B5G6R5Unorm:
			case TextureImageFormat.B5G5R5A1Unorm:
			case TextureImageFormat.A1B5G5R5Unorm:
				return 2;

			case TextureImageFormat.R32Float:
			case TextureImageFormat.R32Uint:
			case TextureImageFormat.R32Sint:
			case TextureImageFormat.R32G32Float:
			case TextureImageFormat.R32G32Uint:
			case TextureImageFormat.R32G32Sint:
			case TextureImageFormat.R32G32B32Float:
			case TextureImageFormat.R32G32B32Uint:
			case TextureImageFormat.R32G32B32Sint:
			case TextureImageFormat.R32G32B32A32Float:
			case TextureImageFormat.R32G32B32A32Uint:
			case TextureImageFormat.R32G32B32A32Sint:
			case TextureImageFormat.R10G10B10A2Unorm:
			case TextureImageFormat.R10G10B10A2Uint:
			case TextureImageFormat.R11G11B10Float:
			case TextureImageFormat.R9G9B9E5Float:
			case TextureImageFormat.R32Uscaled:
			case TextureImageFormat.R32Sscaled:
			case TextureImageFormat.R32G32Uscaled:
			case TextureImageFormat.R32G32Sscaled:
			case TextureImageFormat.R32G32B32Uscaled:
			case TextureImageFormat.R32G32B32Sscaled:
			case TextureImageFormat.R32G32B32A32Uscaled:
			case TextureImageFormat.R32G32B32A32Sscaled:
			case TextureImageFormat.R10G10B10A2Snorm:
			case TextureImageFormat.R10G10B10A2Sint:
			case TextureImageFormat.R10G10B10A2Uscaled:
			case TextureImageFormat.R10G10B10A2Sscaled:
			case TextureImageFormat.B10G10R10A2Unorm:
				return 4;

			case TextureImageFormat.S8Uint:
				return 1;
			case TextureImageFormat.D16Unorm:
				return 2;
			case TextureImageFormat.S8UintD24Unorm:
			case TextureImageFormat.X8UintD24Unorm:
			case TextureImageFormat.D32Float:
			case TextureImageFormat.D24UnormS8Uint:
				return 4;
			case TextureImageFormat.D32FloatS8Uint:
				return 8;

			case TextureImageFormat.Bc1RgbaUnorm:
			case TextureImageFormat.Bc1RgbaSrgb:
				return 8;

			case TextureImageFormat.Bc2Unorm:
			case TextureImageFormat.Bc3Unorm:
			case TextureImageFormat.Bc2Srgb:
			case TextureImageFormat.Bc3Srgb:
			case TextureImageFormat.Bc4Unorm:
			case TextureImageFormat.Bc4Snorm:
			case TextureImageFormat.Bc5Unorm:
			case TextureImageFormat.Bc5Snorm:
			case TextureImageFormat.Bc7Unorm:
			case TextureImageFormat.Bc7Srgb:
			case TextureImageFormat.Bc6HSfloat:
			case TextureImageFormat.Bc6HUfloat:
				return 16;

			case TextureImageFormat.Etc2RgbUnorm:
			case TextureImageFormat.Etc2RgbPtaUnorm:
			case TextureImageFormat.Etc2RgbSrgb:
			case TextureImageFormat.Etc2RgbPtaSrgb:
				return 8;

			case TextureImageFormat.Etc2RgbaUnorm:
			case TextureImageFormat.Etc2RgbaSrgb:
				return 16;

			case TextureImageFormat.Astc4x4Unorm:
			case TextureImageFormat.Astc5x4Unorm:
			case TextureImageFormat.Astc5x5Unorm:
			case TextureImageFormat.Astc6x5Unorm:
			case TextureImageFormat.Astc6x6Unorm:
			case TextureImageFormat.Astc8x5Unorm:
			case TextureImageFormat.Astc8x6Unorm:
			case TextureImageFormat.Astc8x8Unorm:
			case TextureImageFormat.Astc10x5Unorm:
			case TextureImageFormat.Astc10x6Unorm:
			case TextureImageFormat.Astc10x8Unorm:
			case TextureImageFormat.Astc10x10Unorm:
			case TextureImageFormat.Astc12x10Unorm:
			case TextureImageFormat.Astc12x12Unorm:
			case TextureImageFormat.Astc4x4Srgb:
			case TextureImageFormat.Astc5x4Srgb:
			case TextureImageFormat.Astc5x5Srgb:
			case TextureImageFormat.Astc6x5Srgb:
			case TextureImageFormat.Astc6x6Srgb:
			case TextureImageFormat.Astc8x5Srgb:
			case TextureImageFormat.Astc8x6Srgb:
			case TextureImageFormat.Astc8x8Srgb:
			case TextureImageFormat.Astc10x5Srgb:
			case TextureImageFormat.Astc10x6Srgb:
			case TextureImageFormat.Astc10x8Srgb:
			case TextureImageFormat.Astc10x10Srgb:
			case TextureImageFormat.Astc12x10Srgb:
			case TextureImageFormat.Astc12x12Srgb:
				return 16;
		}

		return 1;
	}

	/// <summary>
	/// Checks if the texture format is a depth or depth-stencil format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the format is a depth or depth-stencil format, false otherwise</returns>
	public static bool HasDepth(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.D16Unorm:
			case TextureImageFormat.D24UnormS8Uint:
			case TextureImageFormat.S8UintD24Unorm:
			case TextureImageFormat.X8UintD24Unorm:
			case TextureImageFormat.D32Float:
			case TextureImageFormat.D32FloatS8Uint:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is a stencil or depth-stencil format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the format is a stencil or depth-stencil format, false otherwise</returns>
	public static bool HasStencil(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.D24UnormS8Uint:
			case TextureImageFormat.S8UintD24Unorm:
			case TextureImageFormat.D32FloatS8Uint:
			case TextureImageFormat.S8Uint:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is valid to use as image format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture can be used as image, false otherwise</returns>
	public static bool IsImageCompatible(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.R8Unorm:
			case TextureImageFormat.R8Snorm:
			case TextureImageFormat.R8Uint:
			case TextureImageFormat.R8Sint:
			case TextureImageFormat.R16Float:
			case TextureImageFormat.R16Unorm:
			case TextureImageFormat.R16Snorm:
			case TextureImageFormat.R16Uint:
			case TextureImageFormat.R16Sint:
			case TextureImageFormat.R32Float:
			case TextureImageFormat.R32Uint:
			case TextureImageFormat.R32Sint:
			case TextureImageFormat.R8G8Unorm:
			case TextureImageFormat.R8G8Snorm:
			case TextureImageFormat.R8G8Uint:
			case TextureImageFormat.R8G8Sint:
			case TextureImageFormat.R16G16Float:
			case TextureImageFormat.R16G16Unorm:
			case TextureImageFormat.R16G16Snorm:
			case TextureImageFormat.R16G16Uint:
			case TextureImageFormat.R16G16Sint:
			case TextureImageFormat.R32G32Float:
			case TextureImageFormat.R32G32Uint:
			case TextureImageFormat.R32G32Sint:
			case TextureImageFormat.R8G8B8A8Unorm:
			case TextureImageFormat.R8G8B8A8Snorm:
			case TextureImageFormat.R8G8B8A8Uint:
			case TextureImageFormat.R8G8B8A8Sint:
			case TextureImageFormat.R16G16B16A16Float:
			case TextureImageFormat.R16G16B16A16Unorm:
			case TextureImageFormat.R16G16B16A16Snorm:
			case TextureImageFormat.R16G16B16A16Uint:
			case TextureImageFormat.R16G16B16A16Sint:
			case TextureImageFormat.R32G32B32A32Float:
			case TextureImageFormat.R32G32B32A32Uint:
			case TextureImageFormat.R32G32B32A32Sint:
			case TextureImageFormat.R10G10B10A2Unorm:
			case TextureImageFormat.R10G10B10A2Uint:
			case TextureImageFormat.R11G11B10Float:
			case TextureImageFormat.B8G8R8A8Unorm:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is valid to use as render target color format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture can be used as render target, false otherwise</returns>
	public static bool IsRtColorCompatible(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.R32G32B32A32Float:
			case TextureImageFormat.R32G32B32A32Sint:
			case TextureImageFormat.R32G32B32A32Uint:
			case TextureImageFormat.R16G16B16A16Unorm:
			case TextureImageFormat.R16G16B16A16Snorm:
			case TextureImageFormat.R16G16B16A16Sint:
			case TextureImageFormat.R16G16B16A16Uint:
			case TextureImageFormat.R16G16B16A16Float:
			case TextureImageFormat.R32G32Float:
			case TextureImageFormat.R32G32Sint:
			case TextureImageFormat.R32G32Uint:
			case TextureImageFormat.B8G8R8A8Unorm:
			case TextureImageFormat.B8G8R8A8Srgb:
			case TextureImageFormat.B10G10R10A2Unorm:
			case TextureImageFormat.R10G10B10A2Unorm:
			case TextureImageFormat.R10G10B10A2Uint:
			case TextureImageFormat.R8G8B8A8Unorm:
			case TextureImageFormat.R8G8B8A8Srgb:
			case TextureImageFormat.R8G8B8A8Snorm:
			case TextureImageFormat.R8G8B8A8Sint:
			case TextureImageFormat.R8G8B8A8Uint:
			case TextureImageFormat.R16G16Unorm:
			case TextureImageFormat.R16G16Snorm:
			case TextureImageFormat.R16G16Sint:
			case TextureImageFormat.R16G16Uint:
			case TextureImageFormat.R16G16Float:
			case TextureImageFormat.R11G11B10Float:
			case TextureImageFormat.R32Sint:
			case TextureImageFormat.R32Uint:
			case TextureImageFormat.R32Float:
			case TextureImageFormat.B5G6R5Unorm:
			case TextureImageFormat.B5G5R5A1Unorm:
			case TextureImageFormat.R8G8Unorm:
			case TextureImageFormat.R8G8Snorm:
			case TextureImageFormat.R8G8Sint:
			case TextureImageFormat.R8G8Uint:
			case TextureImageFormat.R16Unorm:
			case TextureImageFormat.R16Snorm:
			case TextureImageFormat.R16Sint:
			case TextureImageFormat.R16Uint:
			case TextureImageFormat.R16Float:
			case TextureImageFormat.R8Unorm:
			case TextureImageFormat.R8Snorm:
			case TextureImageFormat.R8Sint:
			case TextureImageFormat.R8Uint:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is 16 bit packed.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is 16 bit packed, false otherwise</returns>
	public static bool Is16BitPacked(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.B5G6R5Unorm:
			case TextureImageFormat.B5G5R5A1Unorm:
			case TextureImageFormat.R5G5B5X1Unorm:
			case TextureImageFormat.R5G5B5A1Unorm:
			case TextureImageFormat.R5G6B5Unorm:
			case TextureImageFormat.R4G4B4A4Unorm:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is an ASTC format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is an ASTC format, false otherwise</returns>
	public static bool IsAstc(this TextureImageFormat format)
	{
		return format.IsAstcUnorm() || format.IsAstcSrgb();
	}

	/// <summary>
	/// Checks if the texture format is an ASTC Unorm format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is an ASTC Unorm format, false otherwise</returns>
	public static bool IsAstcUnorm(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.Astc4x4Unorm:
			case TextureImageFormat.Astc5x4Unorm:
			case TextureImageFormat.Astc5x5Unorm:
			case TextureImageFormat.Astc6x5Unorm:
			case TextureImageFormat.Astc6x6Unorm:
			case TextureImageFormat.Astc8x5Unorm:
			case TextureImageFormat.Astc8x6Unorm:
			case TextureImageFormat.Astc8x8Unorm:
			case TextureImageFormat.Astc10x5Unorm:
			case TextureImageFormat.Astc10x6Unorm:
			case TextureImageFormat.Astc10x8Unorm:
			case TextureImageFormat.Astc10x10Unorm:
			case TextureImageFormat.Astc12x10Unorm:
			case TextureImageFormat.Astc12x12Unorm:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is an ASTC SRGB format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is an ASTC SRGB format, false otherwise</returns>
	public static bool IsAstcSrgb(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.Astc4x4Srgb:
			case TextureImageFormat.Astc5x4Srgb:
			case TextureImageFormat.Astc5x5Srgb:
			case TextureImageFormat.Astc6x5Srgb:
			case TextureImageFormat.Astc6x6Srgb:
			case TextureImageFormat.Astc8x5Srgb:
			case TextureImageFormat.Astc8x6Srgb:
			case TextureImageFormat.Astc8x8Srgb:
			case TextureImageFormat.Astc10x5Srgb:
			case TextureImageFormat.Astc10x6Srgb:
			case TextureImageFormat.Astc10x8Srgb:
			case TextureImageFormat.Astc10x10Srgb:
			case TextureImageFormat.Astc12x10Srgb:
			case TextureImageFormat.Astc12x12Srgb:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is an ETC2 format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is an ETC2 format, false otherwise</returns>
	public static bool IsEtc2(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.Etc2RgbaSrgb:
			case TextureImageFormat.Etc2RgbaUnorm:
			case TextureImageFormat.Etc2RgbPtaSrgb:
			case TextureImageFormat.Etc2RgbPtaUnorm:
			case TextureImageFormat.Etc2RgbSrgb:
			case TextureImageFormat.Etc2RgbUnorm:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is a BGR format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is a BGR format, false otherwise</returns>
	public static bool IsBgr(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.B5G6R5Unorm:
			case TextureImageFormat.B5G5R5A1Unorm:
			case TextureImageFormat.B8G8R8A8Unorm:
			case TextureImageFormat.B8G8R8A8Srgb:
			case TextureImageFormat.B10G10R10A2Unorm:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is a depth, stencil or depth-stencil format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the format is a depth, stencil or depth-stencil format, false otherwise</returns>
	public static bool IsDepthOrStencil(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.D16Unorm:
			case TextureImageFormat.D24UnormS8Uint:
			case TextureImageFormat.S8UintD24Unorm:
			case TextureImageFormat.X8UintD24Unorm:
			case TextureImageFormat.D32Float:
			case TextureImageFormat.D32FloatS8Uint:
			case TextureImageFormat.S8Uint:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is an unsigned integer color format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is an unsigned integer color format, false otherwise</returns>
	public static bool IsUint(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.R8Uint:
			case TextureImageFormat.R16Uint:
			case TextureImageFormat.R32Uint:
			case TextureImageFormat.R8G8Uint:
			case TextureImageFormat.R16G16Uint:
			case TextureImageFormat.R32G32Uint:
			case TextureImageFormat.R8G8B8Uint:
			case TextureImageFormat.R16G16B16Uint:
			case TextureImageFormat.R32G32B32Uint:
			case TextureImageFormat.R8G8B8A8Uint:
			case TextureImageFormat.R16G16B16A16Uint:
			case TextureImageFormat.R32G32B32A32Uint:
			case TextureImageFormat.R10G10B10A2Uint:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is a signed integer color format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is a signed integer color format, false otherwise</returns>
	public static bool IsSint(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.R8Sint:
			case TextureImageFormat.R16Sint:
			case TextureImageFormat.R32Sint:
			case TextureImageFormat.R8G8Sint:
			case TextureImageFormat.R16G16Sint:
			case TextureImageFormat.R32G32Sint:
			case TextureImageFormat.R8G8B8Sint:
			case TextureImageFormat.R16G16B16Sint:
			case TextureImageFormat.R32G32B32Sint:
			case TextureImageFormat.R8G8B8A8Sint:
			case TextureImageFormat.R16G16B16A16Sint:
			case TextureImageFormat.R32G32B32A32Sint:
			case TextureImageFormat.R10G10B10A2Sint:
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the texture format is an integer color format.
	/// </summary>
	/// <param name="format">Texture format</param>
	/// <returns>True if the texture format is an integer color format, false otherwise</returns>
	public static bool IsInteger(this TextureImageFormat format)
	{
		return format.IsUint() || format.IsSint();
	}

	/// <summary>
	/// Checks if the texture format is a float or sRGB color format.
	/// </summary>
	/// <remarks>
	/// Does not include normalized, compressed or depth formats.
	/// Float and sRGB formats do not participate in logical operations.
	/// </remarks>
	/// <param name="format">Texture format</param>
	/// <returns>True if the format is a float or sRGB color format, false otherwise</returns>
	public static bool IsFloatOrSrgb(this TextureImageFormat format)
	{
		switch (format)
		{
			case TextureImageFormat.R8G8B8A8Srgb:
			case TextureImageFormat.B8G8R8A8Srgb:
			case TextureImageFormat.R16Float:
			case TextureImageFormat.R16G16Float:
			case TextureImageFormat.R16G16B16Float:
			case TextureImageFormat.R16G16B16A16Float:
			case TextureImageFormat.R32Float:
			case TextureImageFormat.R32G32Float:
			case TextureImageFormat.R32G32B32Float:
			case TextureImageFormat.R32G32B32A32Float:
			case TextureImageFormat.R11G11B10Float:
			case TextureImageFormat.R9G9B9E5Float:
				return true;
		}

		return false;
	}
}