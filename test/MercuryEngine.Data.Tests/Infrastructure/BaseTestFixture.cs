using System.Runtime.InteropServices;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Pkg;

namespace MercuryEngine.Data.Tests.Infrastructure;

public abstract class BaseTestFixture
{
	// Store this in a field for faster access
	protected static readonly string RomFsPath = Configuration.RomFsPath;

	private static readonly EnumerationOptions EnumerationOptions = new() {
		MatchCasing = MatchCasing.CaseInsensitive,
		RecurseSubdirectories = true,
	};

	private static string GetTestName(string? basePath, string filePath)
	{
		var relativePath = basePath is null ? filePath : Path.GetRelativePath(basePath, filePath);

		return relativePath.Replace('\\', '$').Replace('/', '$');
	}

	protected static IEnumerable<TestCaseData> GetTestCasesFromRomFs(string fileFormat, string? subDirectory = null)
	{
		var searchPath = Path.Join(RomFsPath, subDirectory);

		return Directory.EnumerateFiles(searchPath, $"*.{fileFormat}", EnumerationOptions)
			.Select(file => new TestCaseData(file) {
				TestName = $"romfs:{GetTestName(RomFsPath, file)}",
			});
	}

	protected static IEnumerable<TestCaseData> GetTestCasesFromPackages(string fileFormat)
	{
		var seenFileNames = new HashSet<string>();

		foreach (var packageFilePath in Directory.EnumerateFiles(RomFsPath, "*.pkg", EnumerationOptions))
		{
			using var fileStream = File.Open(packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			foreach (var file in Pkg.EnumeratePackageFiles(fileStream))
			{
				var fileName = file.Name.ToString();

				if (!seenFileNames.Add(fileName))
					// Already saw this file from another package
					continue;

				if (!fileName.EndsWith($".{fileFormat}", StringComparison.OrdinalIgnoreCase))
					// Wrong kind of file
					continue;

				yield return new TestCaseData(packageFilePath, file) {
					TestName = $"pkg:{GetTestName(null, fileName)}",
				};
			}
		}
	}

	protected static Stream OpenPackageFile(string packageFilePath, PackageFile file)
	{
		var fileStream = File.Open(packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		return Pkg.OpenPackageFile(fileStream, file);
	}

	protected static T ReadWriteAndCompare<T>(string sourceFilePath, string? relativeTo, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		using var fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		return ReadWriteAndCompareCore<T>(fileStream, sourceFilePath, relativeTo, quiet, preCompareAction);
	}

	protected static T ReadWriteAndCompare<T>(string packageFilePath, PackageFile packageFile, string? relativeTo = null, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		using var dataStream = OpenPackageFile(packageFilePath, packageFile);

		return ReadWriteAndCompareCore<T>(dataStream, packageFile.Name.ToString(), relativeTo, quiet, preCompareAction);
	}

	private static T ReadWriteAndCompareCore<T>(Stream dataStream, string sourceFilePath, string? relativeTo, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		var dataMapper = new DataMapper();
		var dataStructure = new T { DataMapper = dataMapper };
		var dataFormatName = dataStructure.DisplayName;

		if (!quiet)
			TestContext.Progress.WriteLine($"Parsing {dataFormatName} file: {sourceFilePath}");

		using var reader = new BinaryReader(dataStream);
		var originalBuffer = reader.ReadBytes((int) dataStream.Length);

		dataStream.Seek(0, SeekOrigin.Begin);

		// Read structure and dump to JSON
		dataStructure.Read(dataStream);
		DataUtilities.DumpDataStructure(dataStructure, sourceFilePath, relativeTo, print: !quiet);

		using var tempStream = new MemoryStream();

		// Write to the temp stream and then rewind it
		dataStructure.Write(tempStream);
		tempStream.Seek(0, SeekOrigin.Begin);

		// Write a copy of the data file back out
		var relativePath = relativeTo is null ? sourceFilePath : Path.GetRelativePath(relativeTo, sourceFilePath);
		var relativeDirectory = Path.GetDirectoryName(relativePath)!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", dataFormatName, relativeDirectory);
		var outFileName = Path.GetFileNameWithoutExtension(sourceFilePath) + ".out" + Path.GetExtension(sourceFilePath);
		var outFilePath = Path.Combine(outFileDir, outFileName);

		if (!quiet)
			TestContext.Progress.WriteLine($"Output {dataFormatName} file: {outFilePath}");

		using (var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write))
			tempStream.CopyTo(outFileStream);

		// Dump the write map
		DataUtilities.DumpDataMapper(dataMapper, outFilePath);

		// Compare the input and output data
		var newBuffer = tempStream.ToArray();

		if (preCompareAction?.Invoke() is not false)
			CompareBuffers(originalBuffer, newBuffer, dataMapper);

		return dataStructure;
	}

	protected static async Task<T> ReadWriteAndCompareAsync<T>(string sourceFilePath, string? relativeTo, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		await using var fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read);

		return await ReadWriteAndCompareAsyncCore<T>(fileStream, sourceFilePath, relativeTo, quiet, preCompareAction);
	}

	protected static async Task<T> ReadWriteAndCompareAsync<T>(string packageFilePath, PackageFile packageFile, string? relativeTo = null, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		await using var dataStream = OpenPackageFile(packageFilePath, packageFile);

		return await ReadWriteAndCompareAsyncCore<T>(dataStream, packageFile.Name.ToString(), relativeTo, quiet, preCompareAction);
	}

	private static async Task<T> ReadWriteAndCompareAsyncCore<T>(Stream dataStream, string sourceFilePath, string? relativeTo, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		var dataMapper = new DataMapper();
		var dataStructure = new T { DataMapper = dataMapper };
		var dataFormatName = dataStructure.DisplayName;

		if (!quiet)
			await TestContext.Progress.WriteLineAsync($"Parsing {dataFormatName} file: {sourceFilePath}");

		using var reader = new BinaryReader(dataStream);
		var originalBuffer = reader.ReadBytes((int) dataStream.Length);

		dataStream.Seek(0, SeekOrigin.Begin);

		// Read structure and dump to JSON
		await dataStructure.ReadAsync(dataStream);
		DataUtilities.DumpDataStructure(dataStructure, sourceFilePath, relativeTo);

		using var tempStream = new MemoryStream();

		// Write to the temp stream and then rewind it
		await dataStructure.WriteAsync(tempStream);
		tempStream.Seek(0, SeekOrigin.Begin);

		// Write a copy of the data file back out
		var relativePath = relativeTo is null ? sourceFilePath : Path.GetRelativePath(relativeTo, sourceFilePath);
		var relativeDirectory = Path.GetDirectoryName(relativePath)!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", dataFormatName, relativeDirectory);
		var outFileName = Path.GetFileNameWithoutExtension(sourceFilePath) + ".out" + Path.GetExtension(sourceFilePath);
		var outFilePath = Path.Combine(outFileDir, outFileName);

		if (!quiet)
			await TestContext.Progress.WriteLineAsync($"Output {dataFormatName} file: {outFilePath}");

		await using (var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write))
			await tempStream.CopyToAsync(outFileStream);

		// Dump the write map
		DataUtilities.DumpDataMapper(dataMapper, outFilePath);

		// Compare the input and output data
		var newBuffer = tempStream.ToArray();

		if (preCompareAction?.Invoke() is not false)
			CompareBuffers(originalBuffer, newBuffer, dataMapper);

		return dataStructure;
	}

	protected internal static void CompareBuffers(byte[] originalBuffer, byte[] newBuffer, DataMapper? dataMapper = null)
	{
		Assert.That(newBuffer, Has.Length.EqualTo(originalBuffer.Length), "New data was a different length than the original data");

		var originalSpan = originalBuffer.AsSpan();
		var newSpan = newBuffer.AsSpan();
		var slowCompareStart = 0;

		if (originalSpan.Length >= sizeof(ulong))
		{
			// Fast comparison by going 8 bytes at a time
			var numChunks = originalSpan.Length / sizeof(ulong);
			var chunkAlignedSize = numChunks * sizeof(ulong);
			var originalSpanAligned = originalSpan[..chunkAlignedSize];
			var newSpanAligned = newSpan[..chunkAlignedSize];

			var originalSpanU64 = MemoryMarshal.Cast<byte, ulong>(originalSpanAligned);
			var newSpanU64 = MemoryMarshal.Cast<byte, ulong>(newSpanAligned);

			// Set "slowCompareStart" to the end of the U64-aligned chunks, in case
			// there are extra bytes at the end (i.e. length not an even multiple of 8).
			// This might get overwritten if there is a mismatch via fast comparison.
			slowCompareStart = chunkAlignedSize;

			for (var i = 0; i < numChunks; i++)
			{
				if (originalSpanU64[i] != newSpanU64[i])
				{
					// Mismatch via fast method - we need to do a byte-by-byte comparison to get the exact offset of the mismatch
					slowCompareStart = i * sizeof(ulong);
					break;
				}
			}
		}

		for (var i = slowCompareStart; i < newBuffer.Length; i++)
		{
			try
			{
				Assert.That(newBuffer[i], Is.EqualTo(originalBuffer[i]), $"Data mismatch at offset {i}");
			}
			catch (AssertionException)
			{
				if (dataMapper != null)
				{
					var failureRangePath = dataMapper.GetContainingRanges((ulong) i);

					TestContext.Error.WriteLine("The pending data assertion failed at the following range in the written data:");
					TestContext.Error.WriteLine(failureRangePath.FormatRangePath());
				}

				throw;
			}
		}
	}
}