using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Converters.Bcmdl;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Types.Pkg;
using SkiaSharp;

namespace MercuryEngine.Data.Tests.Converters;

public class TestMaterialResolver : IMaterialResolver
{
	// Store this in a field for faster access
	protected static readonly string RomFsPath = Configuration.RomFsPath;

	private static readonly EnumerationOptions EnumerationOptions = new() {
		MatchCasing = MatchCasing.CaseInsensitive,
		RecurseSubdirectories = true,
	};

	public Bsmat? LoadMaterial(string path)
	{
		if (!FindPackageFile(path, out var packageFilePath, out var bsmatFile))
			return null;

		using var pkgStream = File.Open(packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var bsmatStream = Pkg.OpenPackageFile(pkgStream, bsmatFile);
		var bsmat = new Bsmat();

		bsmat.Read(bsmatStream);

		return bsmat;
	}

	public SKBitmap? LoadTexture(string path)
	{
		var fullPath = Path.Join(Configuration.RomFsPath, "textures", path);

		if (!File.Exists(fullPath))
		{
			TestContext.Error.WriteLine($"Texture \"{path}\" not found");
			return null;
		}

		var bctex = new Bctex();

		using (var fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			bctex.Read(fileStream);

		if (bctex.Textures.Count == 0)
			return null;

		return bctex.Textures[0].ToBitmap();
	}

	private static bool FindPackageFile(string path, [NotNullWhen(true)] out string? packageFilePath, [NotNullWhen(true)] out PackageFile? file)
	{
		foreach (var curPackageFilePath in Directory.EnumerateFiles(RomFsPath, "*.pkg", EnumerationOptions))
		{
			using var fileStream = File.Open(curPackageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			foreach (var curFile in Pkg.EnumeratePackageFiles(fileStream))
			{
				var fileName = curFile.Name.ToString();

				if (fileName == path)
				{
					packageFilePath = curPackageFilePath;
					file = curFile;
					return true;
				}
			}
		}

		packageFilePath = null;
		file = null;
		return false;
	}
}