using MercuryEngine.Data.Utility;
using NUnit.Framework;

namespace MercuryEngine.Data.Test;

public class Tests
{
	[TestCase("CGameBlackboard")]
	public void TestCrc64(string text)
	{
		var crc64 = Crc64.Calculate(text);

		TestContext.Out.WriteLine($"\"{text}\" = 0x{crc64:x16}");
	}
}