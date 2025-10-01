using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Converters.GameAssets;

public sealed record GameAsset(AssetLocationType LocationType, string RelativePath, string? FullPath, string[]? FullPackageFilePaths)
{
	public GameAsset(string relativePath, string fullRomFsPath)
		: this(AssetLocationType.RomFs, relativePath, fullRomFsPath, null) { }

	public GameAsset(string relativePath, IEnumerable<string> fullPackageFilePaths)
		: this(AssetLocationType.Package, relativePath, null, fullPackageFilePaths.ToArray()) { }

	public StrId RelativePathHash { get; } = RelativePath.GetCrc64();

	public T ReadAs<T>()
	where T : IBinaryFormat, new()
	{
		using var stream = Open();
		var result = new T();

		result.Read(stream);

		return result;
	}

	public Stream Open(bool forWriting = false)
	{
		if (LocationType == AssetLocationType.RomFs)
		{
			if (string.IsNullOrEmpty(FullPath))
				throw new InvalidOperationException($"RomFS asset location missing {nameof(FullPath)}");

			return File.Open(
				FullPath,
				forWriting ? FileMode.Create : FileMode.Open,
				forWriting ? FileAccess.ReadWrite : FileAccess.Read,
				forWriting ? FileShare.Read : FileShare.ReadWrite);
		}

		if (LocationType == AssetLocationType.Package)
		{
			if (forWriting)
				throw new NotSupportedException($"Writing to an asset location with a type of \"{nameof(AssetLocationType.Package)}\" is not supported");

			return OpenFileFromPackages();
		}

		throw new NotSupportedException($"Unrecognized asset location type \"{LocationType}\"");
	}

	private Stream OpenFileFromPackages()
	{
		if (FullPackageFilePaths is null or { Length: 0 })
			throw new InvalidOperationException($"RomFS asset location does not have any {nameof(FullPackageFilePaths)}");

		var fileStreamOptions = new FileStreamOptions {
			Mode = FileMode.Open,
			Access = FileAccess.Read,
			Share = FileShare.Read,
			Options = FileOptions.RandomAccess,
		};
		Stream? currentPackageStream = null;

		try
		{
			foreach (var packageFilePath in FullPackageFilePaths)
			{
				currentPackageStream?.Dispose();
				currentPackageStream = File.Open(packageFilePath, fileStreamOptions);

				var candidateFile = Pkg.EnumeratePackageFiles(currentPackageStream).FirstOrDefault(f => f.Name == RelativePathHash);

				if (candidateFile != null)
					return Pkg.OpenPackageFile(currentPackageStream, candidateFile);
			}

			// If no package contained our file, throw an exception (which will dispose the stream in the catch block below)
			throw new FileNotFoundException($"Could not find asset with path \"{RelativePath}\" in any packages");
		}
		catch
		{
			currentPackageStream?.Dispose();
			throw;
		}
	}
}