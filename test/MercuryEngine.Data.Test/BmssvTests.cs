using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Test.Extensions;
using MercuryEngine.Data.Test.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace MercuryEngine.Data.Test;

[TestFixture]
public partial class BmssvTests
{
	private const string TestFreshProfile         = "Fresh";
	private const string TestHundoProfile         = "Hundo";
	private const string TestRandoWorkingProfile  = "RandoWorking";
	private const string TestRandoBrokenProfile   = "RandoBroken";

	private static IEnumerable<string> GetTestFiles()
	{
		foreach (var profileDirectory in new[] { TestFreshProfile, TestHundoProfile, TestRandoWorkingProfile, TestRandoBrokenProfile }.Select(GetTestProfilePath))
		foreach (var file in Directory.EnumerateFiles(profileDirectory, "*.bmssv", SearchOption.AllDirectories))
		{
			if (Path.GetFileNameWithoutExtension(file).EndsWith("_out", StringComparison.OrdinalIgnoreCase))
				continue;

			yield return file;
		}
	}

	private static string GetTestProfilePath(string profileName)
		=> Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSSV", profileName);

	[TestCase]
	public void FixSaveFile()
	{
		string saveFolder = GetTestProfilePath(TestRandoBrokenProfile);
		string saveFile = Path.Combine(saveFolder, "common.bmssv");
		using var fileStream = File.Open(saveFile, FileMode.Open, FileAccess.Read);
		using var outStream = File.Open(saveFile.Replace(".bmssv", "_fixed.bmssv"), FileMode.Create, FileAccess.Write);
		var bmssv = new Bmssv();

		try
		{
			bmssv.Read(fileStream);

			var inventory = bmssv.Sections["PLAYER_INVENTORY"];

			inventory.PutValue("ITEM_WEAPON_POWER_BOMB", 1f);
			bmssv.Write(outStream);
		}
		finally
		{
			DumpBmssvFile(bmssv, saveFile);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestLoadBmssv(string inFile)
	{
		var filePath = GetTestProfilePath(inFile);
		using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		var bmssv = new Bmssv();

		try
		{
			bmssv.Read(fileStream);
		}
		finally
		{
			DumpBmssvFile(bmssv, filePath);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public void TestCompareBmssv(string inFile)
	{
		var filePath = GetTestProfilePath(inFile);
		using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var bmssv = new Bmssv();

		bmssv.Read(fileStream);

		using var tempStream = new MemoryStream();

		bmssv.Write(tempStream);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		var outFileDir = Path.GetDirectoryName(filePath)!;
		var outFileName = Path.GetFileNameWithoutExtension(filePath) + "_out" + Path.GetExtension(filePath);
		var outFilePath = Path.Combine(outFileDir, outFileName);
		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		tempStream.CopyTo(outFileStream);

		Assert.That(newBuffer.Length, Is.EqualTo(originalBuffer.Length), "New data was a different length than the original data");

		for (var i = 0; i < newBuffer.Length; i++)
			Assert.That(newBuffer[i], Is.EqualTo(originalBuffer[i]), $"Data mismatch at offset {i}");
	}

	private static void DumpBmssvFile(Bmssv bmssv, string bmssvFilePath)
	{
		var serializer = new JsonSerializer {
			Formatting = Formatting.Indented,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Converters = {
				new StringEnumConverter(),
				new TerminatedStringConverter(),
			},
		};
		var jsonObject = JObject.FromObject(bmssv, serializer);

		jsonObject.Sort();

		using var textWriter = new StringWriter();
		using var jsonWriter = new JsonTextWriter(textWriter);

		serializer.Serialize(jsonWriter, jsonObject);

		var jsonDump = textWriter.ToString();

		TestContext.Out.WriteLine("JSON dump of current parsed state:");
		TestContext.Out.WriteLine(jsonDump);

		var outFileDir = Path.GetDirectoryName(bmssvFilePath)!;
		var outFileName = Path.GetFileNameWithoutExtension(bmssvFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);
		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		writer.Write(jsonDump);
	}
}