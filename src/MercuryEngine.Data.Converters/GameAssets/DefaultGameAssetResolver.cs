using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Converters.GameAssets;

/// <summary>
/// Resolves assets using conventional paths and locations from a folder containing an extracted RomFS.
/// </summary>
/// <param name="romFsPath">
/// The full path to a folder containing the extracted contents of Dread's RomFS. This folder should
/// contain, at a minimum, the following folders: &quot;gui&quot;, &quot;packs&quot;, &quot;sounds&quot;,
/// &quot;system&quot;, and &quot;textures&quot;, as well as a &quot;config.ini&quot; file.
/// </param>
public class DefaultGameAssetResolver(string romFsPath) : IGameAssetResolver
{
	private static readonly EnumerationOptions PkgEnumerationOptions = new() {
		MatchCasing = MatchCasing.CaseInsensitive,
		RecurseSubdirectories = true,
	};

	private readonly Dictionary<StrId, GameAsset> packageAssetsCache = [];

	private bool packageCacheBuilt;

	/// <summary>
	/// Gets the path of the RomFS root directory from which game assets are resolved.
	/// </summary>
	public string RomFsPath { get; } = romFsPath;

	/// <summary>
	/// Gets or sets the output path where new or modified RomFS files are written.
	/// </summary>
	public string? OutputPath { get; set; }

	/// <summary>
	/// Gets or sets whether the set of files present in packages is cached for the lifetime of this <see cref="DefaultGameAssetResolver"/>.
	/// </summary>
	/// <remarks>
	/// Default is <see langword="true"/>. This should be set to <see langword="false"/> if the contents of PKG files or of RomFS may change
	/// during the lifetime of this <see cref="DefaultGameAssetResolver"/>.
	/// </remarks>
	public bool UsePackageCache { get; set; } = true;

	public bool TryGetAsset(string relativePath, [NotNullWhen(true)] out GameAsset? assetLocation)
		=> TryGetAsset(relativePath, AssetAccessMode.Read, out assetLocation);

	public bool TryGetAsset(string relativePath, AssetAccessMode accessMode, [NotNullWhen(true)] out GameAsset? assetLocation)
	{
		try
		{
			assetLocation = GetAsset(relativePath, accessMode);
			return true;
		}
		catch
		{
			assetLocation = null;
			return false;
		}
	}

	public GameAsset GetAsset(string relativePath, AssetAccessMode accessMode = AssetAccessMode.Read)
	{
		relativePath = NormalizePath(relativePath);

		// Writing always uses a RomFS location in a separate RomFS hierarchy (in OutputPath)
		bool useOutputPath = accessMode == AssetAccessMode.Write;

		if (accessMode == AssetAccessMode.ReadModified)
		{
			var fullOutputPath = GetFullOutputPath();

			if (File.Exists(fullOutputPath))
				useOutputPath = true;
		}

		if (useOutputPath)
		{
			var fullOutputPath = GetFullOutputPath();

			return new GameAsset(relativePath, fullOutputPath);
		}

		var romfsCandidatePath = Path.Join(RomFsPath, relativePath);

		if (File.Exists(romfsCandidatePath))
			// Asset exists as a bare RomFS file - always prefer that
			return new GameAsset(relativePath, romfsCandidatePath);

		// Look for asset in packages
		var assetFromPackages = FindPackageAsset(relativePath);

		if (assetFromPackages != null)
			return assetFromPackages;

		throw new FileNotFoundException($"Could not find asset with path \"{relativePath}\"", relativePath);

		string GetFullOutputPath()
		{
			if (string.IsNullOrEmpty(OutputPath))
				throw new InvalidOperationException($"{nameof(AssetAccessMode)} \"{accessMode}\" cannot be used because an {nameof(OutputPath)} has not been set");
			if (!Directory.Exists(OutputPath))
				throw new DirectoryNotFoundException($"Output folder \"{OutputPath}\" does not exist");

			return Path.Join(OutputPath, relativePath);
		}
	}

	// TODO: Move to extension method in MercuryEngine.Data.Converters so this class can move to MercuryEngine.Data(.Core?)
	public Bsmat? LoadMaterial(string path)
	{
		if (!TryGetAsset(path, out var bsmatAsset))
			// TODO: Event?
			return null;

		return bsmatAsset.ReadAs<Bsmat>();
	}

	// TODO: Move to extension method in MercuryEngine.Data.Converters so this class can move to MercuryEngine.Data(.Core?)
	public Bctex? LoadTexture(string path)
	{
		// Textures are referenced without the first "textures" folder throughout the game
		var fullRelativePath = Path.Join("textures", path);

		if (!TryGetAsset(fullRelativePath, out var bctexLocation))
			// TODO: Event?
			return null;

		return bctexLocation.ReadAs<Bctex>();
	}

	private GameAsset? FindPackageAsset(string relativePath)
	{
		var assetId = (StrId) NormalizePath(relativePath);

		if (UsePackageCache)
		{
			if (!this.packageCacheBuilt)
			{
				lock (this.packageAssetsCache)
				{
					if (this.packageCacheBuilt)
						// In case somebody else built it while we were waiting for the lock
						return GetAssetFromPackageCache(assetId);

					BuildPackageCache();
					this.packageCacheBuilt = true;
				}
			}

			return GetAssetFromPackageCache(assetId);
		}

		var packagesWithFile = FindPackagesWithFileUncached(assetId).ToList();

		return packagesWithFile.Count > 0 ? new GameAsset(assetId, packagesWithFile) : null;
	}

	private GameAsset? GetAssetFromPackageCache(StrId assetId)
		=> this.packageAssetsCache.GetValueOrDefault(assetId);

	private void BuildPackageCache()
	{
		var packagesWithFile = new Dictionary<StrId, List<string>>();

		foreach (var curPackageFilePath in Directory.EnumerateFiles(RomFsPath, "*.pkg", PkgEnumerationOptions))
		{
			using var packageStream = File.Open(curPackageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			foreach (var packageFileEntry in Pkg.EnumeratePackageFiles(packageStream))
			{
				if (!packagesWithFile.TryGetValue(packageFileEntry.Name, out var packagesList))
				{
					packagesList = [];
					packagesWithFile[packageFileEntry.Name] = packagesList;
				}

				packagesList.Add(curPackageFilePath);
			}
		}

		this.packageAssetsCache.Clear();

		foreach (var (assetId, packages) in packagesWithFile)
			this.packageAssetsCache.Add(assetId, new GameAsset(assetId, packages));
	}

	private IEnumerable<string> FindPackagesWithFileUncached(StrId assetId)
	{
		foreach (var curPackageFilePath in Directory.EnumerateFiles(RomFsPath, "*.pkg", PkgEnumerationOptions))
		{
			using var fileStream = File.Open(curPackageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			foreach (var candidate in Pkg.EnumeratePackageFiles(fileStream))
			{
				if (candidate.Name == assetId)
				{
					yield return curPackageFilePath;
					break; // Move onto the next package
				}
			}
		}
	}

	private static string NormalizePath(string path)
	{
		var normalized = path.Replace('\\', '/');

		if (normalized.StartsWith("./"))
			// Can't use "TrimStart", as it could erroneously remove "../"
			normalized = normalized[2..];

		return normalized.TrimStart('/');
	}
}