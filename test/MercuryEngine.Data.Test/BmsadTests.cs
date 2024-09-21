using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Test.Extensions;
using MercuryEngine.Data.Test.Utility;
using MercuryEngine.Data.Test.Utility.Json;

namespace MercuryEngine.Data.Test;

[TestFixture]
public class BmsadTests
{
	private static readonly string PackagesPath = Configuration.PackagesPath; // Store it for faster access

	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var bmsadFile in Directory.EnumerateFiles(PackagesPath, "*.bmsad", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(PackagesPath, bmsadFile);

			yield return new TestCaseData(bmsadFile) {
				TestName = relativePath,
			};
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBmsad(string inFile)
	{
		TestContext.Progress.WriteLine("Loading BMSAD file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bmsad = new Bmsad();

		try
		{
			bmsad.Read(fileStream);
		}
		finally
		{
			DumpBmsadFile(bmsad, inFile);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBmsad(string inFile)
	{
		TestContext.Progress.WriteLine("Parsing BMSAD file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var dataMapper = new DataMapper();
		var bmsad = new Bmsad {
			DataMapper = dataMapper,
		};

		bmsad.Read(fileStream);

		using var tempStream = new MemoryStream();

		bmsad.Write(tempStream);

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, inFile))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSAD", relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(inFile) + "_out" + Path.GetExtension(inFile);
		var outFilePath = Path.Combine(outFileDir, outFileName);

		TestContext.Progress.WriteLine("Output BMSAD file: {0}", outFilePath);

		DataUtilities.DumpDataMapper(dataMapper, outFilePath);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		tempStream.CopyTo(outFileStream);

		Assert.That(newBuffer.Length, Is.EqualTo(originalBuffer.Length), "New data was a different length than the original data");

		for (var i = 0; i < newBuffer.Length; i++)
		{
			try
			{
				Assert.That(newBuffer[i], Is.EqualTo(originalBuffer[i]), $"Data mismatch at offset {i}");
			}
			catch (AssertionException)
			{
				var failureRangePath = dataMapper.GetContainingRanges((ulong) i);

				TestContext.Error.WriteLine("The pending data assertion failed at the following range in the written data:");
				TestContext.Error.WriteLine(failureRangePath.FormatRangePath());

				throw;
			}
		}
	}

	private static void DumpBmsadFile(Bmsad bmsad, string bmsadFilePath)
	{
		var jsonObject = JsonSerializer.SerializeToNode(bmsad, JsonUtility.JsonOptions)!.AsObject();

		jsonObject.Sort();

		var jsonDump = jsonObject.ToJsonString(JsonUtility.JsonOptions);

		TestContext.Out.WriteLine("JSON dump of current parsed state:");
		TestContext.Out.WriteLine(jsonDump);

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(PackagesPath, bmsadFilePath))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSAD", relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(bmsadFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		writer.Write(jsonDump);
	}
}