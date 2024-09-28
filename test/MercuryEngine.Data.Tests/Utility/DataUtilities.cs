using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Tests.Utility.Json;
using System.Text;
using System.Text.Json;

namespace MercuryEngine.Data.Tests.Utility;

internal static class DataUtilities
{
	public static string DumpDataStructure<T>(T dataStructure, string sourceFilePath, string relativeTo)
	where T : DataStructure<T>
	{
		var jsonDump = JsonSerializer.Serialize(dataStructure, JsonUtility.JsonOptions);

		var relativePath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo, sourceFilePath))!;
		var dataFormatName = typeof(T).Name.ToUpper();
		var outFileDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", dataFormatName, relativePath);
		var outFileName = Path.GetFileNameWithoutExtension(sourceFilePath) + ".json";
		var outFilePath = Path.Combine(outFileDir, outFileName);

		Directory.CreateDirectory(outFileDir);

		using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

		writer.Write(jsonDump);

		return jsonDump;
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