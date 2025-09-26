using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Infrastructure;

internal static class PkgReader
{
	[SuppressMessage(
		"ReSharper",
		"ConvertToConstant.Global",
		Justification = "Used as a toggleable flag that shouldn't result in \"heuristically unreachable code\" warnings")]
	public static readonly bool BufferPackagesToMemory = true;

	/// <summary>
	/// We nest a <see cref="Lazy{T}"/> in the concurrent dictionary because a <see cref="ConcurrentDictionary{TKey,TValue}"/> can
	/// end up invoking the factory method twice at once if two threads simultaneously try to call GetOrAdd. One of the two calls
	/// will be discarded, but with PKG files, that could be a lot of wasted memory and I/O. By using <see cref="Lazy{T}"/>, the
	/// worst that can happen is that two lazy instances get created and one is discarded. Then the lazy instance that is kept will
	/// read the PKG buffer once.
	/// </summary>
	private static readonly ConcurrentDictionary<string, Lazy<byte[]>> PackageFileBuffers = [];

	public static Stream OpenPackageFile(string packageFilePath, PackageFile file)
	{
		Stream pkgStream;

		if (!BufferPackagesToMemory)
		{
			var options = new FileStreamOptions {
				Mode = FileMode.Open,
				Access = FileAccess.Read,
				Share = FileShare.Read,
				Options = FileOptions.RandomAccess,
				BufferSize = (int) BitOperations.RoundUpToPowerOf2((uint) file.Length),
			};
			pkgStream = File.Open(packageFilePath, options);

			return Pkg.OpenPackageFile(pkgStream, file);
		}

		var packageBufferLazy = PackageFileBuffers.GetOrAdd(packageFilePath, CreatePackageFileBufferLazy);
		var packageBuffer = packageBufferLazy.Value;

		pkgStream = new MemoryStream(packageBuffer);

		return Pkg.OpenPackageFile(pkgStream, file);
	}

	private static Lazy<byte[]> CreatePackageFileBufferLazy(string packageFilePath)
		=> new(() => {
			using var pkgStream = File.Open(packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var memoryStream = new MemoryStream((int) pkgStream.Length);

			pkgStream.CopyTo(memoryStream);

			return memoryStream.ToArray();
		}, LazyThreadSafetyMode.ExecutionAndPublication);
}