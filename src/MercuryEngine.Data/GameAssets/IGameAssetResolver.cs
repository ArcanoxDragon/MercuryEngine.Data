namespace MercuryEngine.Data.GameAssets;

public interface IGameAssetResolver
{
	/// <summary>
	/// Tries to locate an existing asset with the provided <paramref name="relativePath"/> and
	/// returns it via <paramref name="assetLocation"/>. The return value of this method indicates
	/// whether or not the asset exists on the filesystem.
	/// </summary>
	/// <param name="relativePath">
	/// <inheritdoc cref="GetAsset" path="/param[@name='relativePath']/text()"/>
	/// </param>
	/// <param name="assetLocation">
	/// Will be populated with a <see cref="GameAsset"/> representing the requested asset.
	/// </param>
	/// <param name="includeOutputAssets">
	/// Whether or not to consider assets that do not exist in the base game files, but that do
	/// exist in a configured output path (where modified assets are written), when determining
	/// whether or not the requested asset exists.
	/// </param>
	/// <returns>
	/// <para>
	/// If <paramref name="includeOutputAssets"/> is <see langword="true"/>, this method returns
	/// <see langword="true"/> if an asset with the requested <paramref name="relativePath"/>
	/// exists in the base RomFS, base packages, or a configured output RomFS location.
	/// </para>
	/// <para>
	/// If <paramref name="includeOutputAssets"/> is <see langword="false"/>, this method returns
	/// <see langword="true"/> only if an asset with the requested <paramref name="relativePath"/>
	/// exists in the base RomFS or base packages, and returns <see langword="false"/> if not (even
	/// if a matching asset does exist in a configured output location).
	/// </para>
	/// </returns>
	bool TryGetExistingAsset(string relativePath, out GameAsset assetLocation, bool includeOutputAssets = false);

	/// <summary>
	/// Returns a <see cref="GameAsset"/> instance representing the asset with the requested <paramref name="relativePath"/>.
	/// </summary>
	/// <remarks>
	/// If the requested asset exists in the base game's RomFS, the <see cref="GameAsset"/> will have a
	/// <see cref="GameAsset.Location"/> of <see cref="AssetLocation.RomFs"/>. If the asset exists in the
	/// base game's PKG files, it will have a <see cref="GameAsset.Location"/> of <see cref="AssetLocation.Package"/>.
	/// If the asset exists in neither of those locations, it will have a <see cref="GameAsset.Location"/> of
	/// <see cref="AssetLocation.None"/>, even if a matching asset does exist in a configure output location.
	/// </remarks>
	/// <param name="relativePath">
	/// The path to the requested asset, relative to the root of the game's RomFS.
	/// </param>
	/// <param name="assetIdOverride">
	/// If provided, the asset's <see cref="GameAsset.AssetId"/> property will be set to the value of this parameter
	/// instead of the default value (its relative path). This can be used, for example, in cases where the ID of the
	/// asset in the <c>files.toc</c> file must differ from its actual relative path (such as is the case with BCTEX
	/// files).
	/// </param>
	/// <returns>
	/// A <see cref="GameAsset"/> instance representing the asset with the requested <paramref name="relativePath"/>.
	/// </returns>
	GameAsset GetAsset(string relativePath, string? assetIdOverride = null);

	// TODO: Docs
	IEnumerable<GameAsset> EnumerateAssets(string directory, Func<string, string>? assetIdTransformer = null);
}