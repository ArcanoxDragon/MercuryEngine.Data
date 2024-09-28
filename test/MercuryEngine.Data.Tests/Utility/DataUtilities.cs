using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Tests.Utility.Json;
using System.Text;
using System.Text.Json;
using MercuryEngine.Data.Core.Framework;

namespace MercuryEngine.Data.Tests.Utility;

internal static class DataUtilities
{
	public static void DumpDataStructure<T>(T dataStructure, string sourceFilePath, string relativeTo, bool print = true)
	where T : BinaryFormat<T>, new()
	{
		var formatName = dataStructure.DisplayName;
		var jsonDump = JsonSerializer.Serialize(dataStructure, JsonUtility.JsonOptions);

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", dataStructure.DisplayName, relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(sourceFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		writer.Write(jsonDump);

		if (print)
		{
			TestContext.Out.WriteLine($"JSON dump of current parsed {formatName} state:");
			TestContext.Out.WriteLine(jsonDump);
		}
	}

	public static void DumpDataMapper(DataMapper dataMapper, string dataFilePath)
	{
		var outFileDir = Path.GetDirectoryName(dataFilePath)!;
		var outFileName = Path.GetFileNameWithoutExtension(dataFilePath) + ".map.json";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		JsonSerializer.Serialize(outFileStream, dataMapper, JsonUtility.JsonOptions);
	}
}