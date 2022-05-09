using System;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Test;

public class Tests
{
	[TestCase("CGameBlackboard")]
	[TestCase("collision_base")]
	public void TestCrc64(string text)
	{
		var crc64 = Crc64.Calculate(text);
		var crc64Hex = BitConverter.GetBytes(crc64).ToHexString();

		TestContext.Out.WriteLine($"\"{text}\" = {crc64Hex}");
	}
}