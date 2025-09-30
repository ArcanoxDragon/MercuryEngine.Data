namespace MercuryEngine.Data.Converters.GameAssets;

/// <summary>
/// Describes the type of location where a game asset is stored.
/// </summary>
public enum AssetLocationType
{
	/// <summary>
	/// The asset is located as a bare file in the game's RomFS.
	/// </summary>
	RomFs,

	/// <summary>
	/// The asset is located inside of one or more PKG files.
	/// </summary>
	Package,
}