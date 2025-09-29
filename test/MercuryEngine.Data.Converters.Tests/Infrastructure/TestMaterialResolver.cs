using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Converters.Bcmdl;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Converters.Tests.Infrastructure;

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

	public Bctex? LoadTexture(string path)
	{
		var fullPath = Path.Join(Configuration.RomFsPath, "textures", path);

		if (!File.Exists(fullPath))
		{
			TestContext.Error.WriteLine($"Texture \"{path}\" not found");
			return null;
		}

		using var fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bctex = new Bctex();

		bctex.Read(fileStream);

		return bctex;
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