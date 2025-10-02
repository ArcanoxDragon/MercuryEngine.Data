namespace MercuryEngine.Data.GameAssets;

/// <summary>
/// Describes the type of location where a game asset is stored.
/// </summary>
public enum AssetLocation
{
	/// <summary>
	/// The asset is located as a bare file in the game's RomFS.
	/// </summary>
	RomFs,

	/// <summary>
	/// The asset is located inside of one or more PKG files.
	/// </summary>
	Package,

	/// <summary>
	/// The asset does not exist in either RomFS or any PKG files.
	/// This is also used for new/custom assets that do not exist in the base game,
	/// but that may be written to a custom RomFS (e.g. for inclusion with a mod).
	/// </summary>
	None,
}