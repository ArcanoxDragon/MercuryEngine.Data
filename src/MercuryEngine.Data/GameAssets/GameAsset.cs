using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.GameAssets;

public sealed record GameAsset(AssetLocation Location, string RelativePath, StrId AssetId, string? FullPath, string[]? FullPackageFilePaths)
{
	public GameAsset(string relativePath, string? assetIdOverride = null)
		: this(AssetLocation.None, relativePath, relativePath, null, null)
	{
		if (assetIdOverride != null)
			AssetId = assetIdOverride;
	}

	public GameAsset(string relativePath, string fullRomFsPath, string? assetIdOverride = null)
		: this(AssetLocation.RomFs, relativePath, relativePath, fullRomFsPath, null)
	{
		if (assetIdOverride != null)
			AssetId = assetIdOverride;
	}

	public GameAsset(StrId assetId, IEnumerable<string> fullPackageFilePaths)
		: this(AssetLocation.Package, assetId.IsKnown ? assetId.StringValue : string.Empty, assetId, null, fullPackageFilePaths.ToArray())
	{
		AssetId = assetId;
	}

	public string? OutputPath { get; init; }

	/// <summary>
	/// Gets whether or not this asset exists in either the base game files or the configured RomFS output path.
	/// </summary>
	public bool Exists => ExistsInBaseGame || ExistsInOutput;

	/// <summary>
	/// Gets whether or not this asset exists in the base game files (either in RomFS directly, or in a PKG file)
	/// </summary>
	public bool ExistsInBaseGame => Location switch {
		AssetLocation.RomFs   => File.Exists(FullPath),
		AssetLocation.Package => FullPackageFilePaths?.Length > 0,
		_                     => false,
	};

	/// <summary>
	/// Gets whether or not this asset exists in the configured RomFS output path.
	/// </summary>
	public bool ExistsInOutput => File.Exists(OutputPath);

	// TODO: Documentation for ReadAs/Write/OpenRead/OpenWrite

	public T ReadAs<T>(bool useExistingOutput = false)
	where T : IBinaryFormat, new()
	{
		using var stream = OpenRead(useExistingOutput);
		var result = new T();

		result.Read(stream);

		return result;
	}

	public Task<T> ReadAsAsync<T>(CancellationToken cancellationToken)
	where T : IBinaryFormat, new()
		=> ReadAsAsync<T>(false, cancellationToken);

	public async Task<T> ReadAsAsync<T>(bool useExistingOutput = false, CancellationToken cancellationToken = default)
	where T : IBinaryFormat, new()
	{
		await using var stream = OpenRead(useExistingOutput, async: true);
		var result = new T();

		await result.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

		return result;
	}

	public uint Write<T>(T data)
	where T : IBinaryFormat
	{
		using var stream = OpenWrite();

		data.Write(stream);
		stream.Flush();

		return (uint) stream.Position;
	}

	public async Task<uint> WriteAsync<T>(T data, CancellationToken cancellationToken = default)
	where T : IBinaryFormat
	{
		await using var stream = OpenWrite(async: true);

		await data.WriteAsync(stream, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

		return (uint) stream.Position;
	}

	public Stream OpenRead(bool useExistingOutput = false, bool async = false)
		=> OpenCore(false, useExistingOutput, async);

	public Stream OpenWrite(bool async = false)
		=> OpenCore(true, async: async);

	private Stream OpenCore(bool forWriting, bool readExistingOutput = false, bool async = false)
	{
		if (forWriting && string.IsNullOrEmpty(OutputPath))
			throw new InvalidOperationException($"Cannot write asset because \"{nameof(OutputPath)}\" has not been set");

		bool useOutputPath = forWriting || ( readExistingOutput && File.Exists(OutputPath) );

		if (useOutputPath)
		{
			if (Path.GetDirectoryName(OutputPath) is { } directoryName)
				Directory.CreateDirectory(directoryName);

			var fileStreamOptions = new FileStreamOptions {
				Options = async ? FileOptions.Asynchronous : FileOptions.None,
				Mode = forWriting ? FileMode.Create : FileMode.Open,
				Access = forWriting ? FileAccess.ReadWrite : FileAccess.Read,
				Share = forWriting ? FileShare.Read : FileShare.ReadWrite,
			};

			return File.Open(OutputPath!, fileStreamOptions);
		}

		if (Location == AssetLocation.RomFs)
		{
			if (forWriting)
				// Shouldn't be able to get here, but just in case
				throw new NotSupportedException("Writing to base RomFS is not supported");

			if (string.IsNullOrEmpty(FullPath))
				throw new InvalidOperationException($"Cannot read {nameof(AssetLocation.RomFs)} asset because \"{nameof(FullPath)}\" is not set");

			var fileStreamOptions = new FileStreamOptions {
				Options = async ? FileOptions.Asynchronous : FileOptions.None,
				Mode = FileMode.Open,
				Access = FileAccess.Read,
				Share = FileShare.ReadWrite,
			};

			return File.Open(FullPath, fileStreamOptions);
		}

		if (Location == AssetLocation.Package)
		{
			if (forWriting)
				throw new NotSupportedException($"Writing to an asset location with a type of \"{nameof(AssetLocation.Package)}\" is not supported");

			return OpenFileFromPackages(async);
		}

		throw new NotSupportedException($"Cannot read asset \"{RelativePath}\" as it does not exist");
	}

	private Stream OpenFileFromPackages(bool async)
	{
		if (FullPackageFilePaths is null or { Length: 0 })
			throw new InvalidOperationException($"RomFS asset location does not have any {nameof(FullPackageFilePaths)}");

		var fileStreamOptions = new FileStreamOptions {
			Options = FileOptions.RandomAccess | ( async ? FileOptions.Asynchronous : FileOptions.None ),
			Mode = FileMode.Open,
			Access = FileAccess.Read,
			Share = FileShare.Read,
		};
		Stream? currentPackageStream = null;

		try
		{
			foreach (var packageFilePath in FullPackageFilePaths)
			{
				currentPackageStream?.Dispose();
				currentPackageStream = File.Open(packageFilePath, fileStreamOptions);

				var candidateFile = Pkg.EnumeratePackageFiles(currentPackageStream).FirstOrDefault(f => f.Name == AssetId);

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