namespace MercuryEngine.Data.Converters.GameAssets;

/// <summary>
/// Determines where and how to look for game assets.
/// </summary>
public enum AssetAccessMode
{
	/// <summary>
	/// Assets are resolved from the base game files - either from RomFS directly, or from a PKG file,
	/// depending on where a particular asset is located.
	/// </summary>
	Read,

	/// <summary>
	/// Assets are resolved relative to a configured output path (separate from the base game files)
	/// that mirrors the base game's RomFS structure. This is used when writing assets so that base
	/// game files are not overwritten.
	/// </summary>
	Write,

	/// <summary>
	/// Like <see cref="Read"/>, except that if an output path has been configured and a requested asset
	/// already exists in the output path, the modified version in the output path will be used instead.
	/// This can be used to read-back assets that have been modified elsewhere.
	/// </summary>
	ReadModified,
}