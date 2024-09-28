using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Utility;

namespace MercuryEngine.Data.Tests;

[TestFixture]
public partial class BmssvTests
{
	[TestCaseSource(nameof(GetTestFiles))]
	public async Task TestLoadBmssvAsync(string inFile)
	{
		var sourceFilePath = GetTestProfilePath(inFile);
		await using var fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read);
		var bmssv = new Bmssv();

		try
		{
			await bmssv.ReadAsync(fileStream);
		}
		finally
		{
			DataUtilities.DumpDataStructure(bmssv, sourceFilePath, BaseDirectory);
		}
	}

	[TestCaseSource(nameof(GetTestFiles))]
	public async Task TestCompareBmssvAsync(string inFile)
	{
		var sourceFilePath = GetTestProfilePath(inFile);

		await ReadWriteAndCompareAsync<Bmssv>(sourceFilePath, BaseDirectory);
	}
}