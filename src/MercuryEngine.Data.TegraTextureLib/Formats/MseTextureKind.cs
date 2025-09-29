namespace MercuryEngine.Data.TegraTextureLib.Formats;

public enum MseTextureKind : ushort
{
	Unknown,

	/// <summary>
	/// DXT1 or BGRA8
	/// </summary>
	OpaqueRgb,

	/// <summary>
	/// DXT5 or RGBA8
	/// </summary>
	Rgba,

	/// <summary>
	/// R8
	/// </summary>
	OneChannel = 4,

	/// <summary>
	/// R8G8
	/// </summary>
	TwoChannel,

	/// <summary>
	/// BC5U
	/// </summary>
	TwoChannelCompressed = 0xC,

	/// <summary>
	/// BC6U
	/// </summary>
	Hdr = 0xF,
}