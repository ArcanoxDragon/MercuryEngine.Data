using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;

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

	/// <summary>
	/// Gets the path of the RomFS root directory from which game assets are resolved.
	/// </summary>
	public string RomFsPath { get; } = romFsPath;

	/// <summary>
	/// Gets or sets the output path where new or modified RomFS files are written.
	/// </summary>
	public string? OutputPath { get; set; }

	public bool TryGetAsset(string relativePath, [NotNullWhen(true)] out GameAsset? assetLocation, bool forWriting = false)
	{
		try
		{
			assetLocation = GetAsset(relativePath, forWriting);
			return true;
		}
		catch
		{
			assetLocation = null;
			return false;
		}
	}

	public GameAsset GetAsset(string relativePath, bool forWriting = false)
	{
		relativePath = NormalizePath(relativePath);

		if (forWriting)
		{
			// Writing always uses RomFS

			if (string.IsNullOrEmpty(OutputPath))
				throw new InvalidOperationException($"Assets cannot be written because an {nameof(OutputPath)} has not been set");
			if (!Directory.Exists(OutputPath))
				throw new DirectoryNotFoundException($"Output folder \"{OutputPath}\" does not exist");

			var fullOutputPath = Path.Join(OutputPath, relativePath);

			return new GameAsset(relativePath, fullOutputPath);
		}

		var romfsCandidatePath = Path.Join(RomFsPath, relativePath);

		if (File.Exists(romfsCandidatePath))
			// Asset exists as a bare RomFS file - always prefer that
			return new GameAsset(relativePath, romfsCandidatePath);

		// Look for asset in packages
		var packagesWithFile = FindPackagesWithFile(relativePath).ToList();

		if (packagesWithFile.Count > 0)
			return new GameAsset(relativePath, packagesWithFile);

		throw new FileNotFoundException($"Could not find asset with path \"{relativePath}\"", relativePath);
	}

	public Bsmat? LoadMaterial(string path)
	{
		if (!TryGetAsset(path, out var bsmatAsset))
			// TODO: Event?
			return null;

		return bsmatAsset.ReadAs<Bsmat>();
	}

	public Bctex? LoadTexture(string path)
	{
		// Textures are referenced without the first "textures" folder throughout the game
		var fullRelativePath = Path.Join("textures", path);

		if (!TryGetAsset(fullRelativePath, out var bctexLocation))
			// TODO: Event?
			return null;

		return bctexLocation.ReadAs<Bctex>();
	}

	private IEnumerable<string> FindPackagesWithFile(string relativePath)
	{
		var relativePathCrc = NormalizePath(relativePath).GetCrc64();

		foreach (var curPackageFilePath in Directory.EnumerateFiles(RomFsPath, "*.pkg", PkgEnumerationOptions))
		{
			using var fileStream = File.Open(curPackageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			foreach (var candidate in Pkg.EnumeratePackageFiles(fileStream))
			{
				if (candidate.Name.Value == relativePathCrc)
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