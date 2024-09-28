using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests.Infrastructure;

public abstract class BaseTestFixture
{
	// Store these in fields for faster access
	protected static readonly string PackagesPath = Configuration.PackagesPath;
	protected static readonly string RomFsPath    = Configuration.RomFsPath;

	private static readonly EnumerationOptions EnumerationOptions = new() {
		MatchCasing = MatchCasing.CaseInsensitive,
		RecurseSubdirectories = true,
	};

	protected static IEnumerable<TestCaseData> GetTestCasesFromRomFs(string fileFormat)
		=> Directory.EnumerateFiles(RomFsPath, $"*.{fileFormat}", EnumerationOptions)
			.Select(file => new TestCaseData(file) {
				TestName = Path.GetRelativePath(RomFsPath, file),
			});

	protected static IEnumerable<TestCaseData> GetTestCasesFromPackages(string fileFormat)
		=> Directory.EnumerateFiles(PackagesPath, $"*.{fileFormat}", EnumerationOptions)
			.Select(file => new TestCaseData(file) {
				TestName = Path.GetRelativePath(PackagesPath, file),
			});

	protected static void ReadWriteAndCompare<T>(string sourceFilePath, string relativeTo, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		var dataMapper = new DataMapper();
		var dataStructure = new T { DataMapper = dataMapper };
		var dataFormatName = dataStructure.DisplayName;

		if (!quiet)
			TestContext.Progress.WriteLine($"Parsing {dataFormatName} file: {sourceFilePath}");

		using var fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		// Read structure and dump to JSON
		dataStructure.Read(fileStream);
		DataUtilities.DumpDataStructure(dataStructure, sourceFilePath, relativeTo);

		using var tempStream = new MemoryStream();

		// Write to the temp stream and then rewind it
		dataStructure.Write(tempStream);
		tempStream.Seek(0, SeekOrigin.Begin);

		// Write a copy of the data file back out
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", dataFormatName, relativePath);
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

		if (preCompareAction?.Invoke() is false)
			// Skip comparison
			return;

		CompareBuffers(originalBuffer, newBuffer, dataMapper);
	}

	protected static async Task ReadWriteAndCompareAsync<T>(string sourceFilePath, string relativeTo, bool quiet = false, Func<bool>? preCompareAction = null)
	where T : BinaryFormat<T>, new()
	{
		var dataMapper = new DataMapper();
		var dataStructure = new T { DataMapper = dataMapper };
		var dataFormatName = dataStructure.DisplayName;

		if (!quiet)
			await TestContext.Progress.WriteLineAsync($"Parsing {dataFormatName} file: {sourceFilePath}");

		await using var fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		// Read structure and dump to JSON
		await dataStructure.ReadAsync(fileStream);
		DataUtilities.DumpDataStructure(dataStructure, sourceFilePath, relativeTo);

		using var tempStream = new MemoryStream();

		// Write to the temp stream and then rewind it
		await dataStructure.WriteAsync(tempStream);
		tempStream.Seek(0, SeekOrigin.Begin);

		// Write a copy of the data file back out
		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", dataFormatName, relativePath);
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

		if (preCompareAction?.Invoke() is false)
			// Skip comparison
			return;

		CompareBuffers(originalBuffer, newBuffer, dataMapper);
	}

	protected static void CompareBuffers(byte[] originalBuffer, byte[] newBuffer, DataMapper? dataMapper = null)
	{
		Assert.That(newBuffer, Has.Length.EqualTo(originalBuffer.Length), "New data was a different length than the original data");

		for (var i = 0; i < newBuffer.Length; i++)
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