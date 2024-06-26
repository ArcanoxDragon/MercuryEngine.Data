﻿using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Test.Extensions;
using MercuryEngine.Data.Test.Utility;

namespace MercuryEngine.Data.Test;

[TestFixture]
public partial class BmssvTests
{
	[TestCaseSource(nameof(GetTestFiles))]
	public async Task TestLoadBmssvAsync(string inFile)
	{
		var filePath = GetTestProfilePath(inFile);
		await using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		var bmssv = new Bmssv();

		try
		{
			await bmssv.ReadAsync(fileStream);
		}
		finally
		{
			var jsonObject = JsonSerializer.SerializeToNode(bmssv, JsonUtility.JsonOptions)!.AsObject();

			jsonObject.Sort();

			var jsonDump = jsonObject.ToJsonString(JsonUtility.JsonOptions);

			await TestContext.Out.WriteLineAsync("JSON dump of current parsed state:");
			await TestContext.Out.WriteLineAsync(jsonDump);

			var outFileDir = Path.GetDirectoryName(filePath)!;
			var outFileName = Path.GetFileNameWithoutExtension(filePath) + ".json";
			var outFilePath = Path.Combine(outFileDir, outFileName);
			await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);
			await using var writer = new StreamWriter(outFileStream, Encoding.UTF8);

			await writer.WriteAsync(jsonDump);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public async Task TestCompareBmssvAsync(string inFile)
	{
		var filePath = GetTestProfilePath(inFile);
		await using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(fileStream);
		var originalBuffer = reader.ReadBytes((int) fileStream.Length);

		fileStream.Seek(0, SeekOrigin.Begin);

		var dataMapper = new DataMapper();
		var bmssv = new Bmssv {
			DataMapper = dataMapper,
		};

		await bmssv.ReadAsync(fileStream);

		using var tempStream = new MemoryStream();

		await bmssv.WriteAsync(tempStream);

		DumpDataMapper(dataMapper, filePath);
		tempStream.Seek(0, SeekOrigin.Begin);

		var newBuffer = tempStream.ToArray();
		var outFileDir = Path.GetDirectoryName(filePath)!;
		var outFileName = Path.GetFileNameWithoutExtension(filePath) + "_out" + Path.GetExtension(filePath);
		var outFilePath = Path.Combine(outFileDir, outFileName);
		await using var outFileStream = File.Open(outFilePath, FileMode.Create, FileAccess.Write);

		await tempStream.CopyToAsync(outFileStream);

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

				await TestContext.Error.WriteLineAsync("The pending data assertion failed at the following range in the written data:");
				await TestContext.Error.WriteLineAsync(failureRangePath.FormatRangePath());

				throw;
			}
		}
	}
}