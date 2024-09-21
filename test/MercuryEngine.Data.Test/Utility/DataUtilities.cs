using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Test.Utility.Json;
using System.IO;
using System.Text.Json;

namespace MercuryEngine.Data.Test.Utility;

internal static class DataUtilities
{
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