using MercuryEngine.Data.Tests.Infrastructure;

namespace MercuryEngine.Data.Tests;

[TestFixture]
public class TestTests
{
	private static readonly IEnumerable<TestCaseData> CompareBuffersTestCases = [
		new TestCaseData(new byte[] { 0x00, 0x01, 0xFF }, new byte[] { 0x00, 0x01, 0xFF }, null)
			.SetName("Buffer is shorter than U64 - same data"),

		new TestCaseData(new byte[] { 0x00, 0x01, 0xFF }, new byte[] { 0x00, 0x01, 0xFE }, 2)
			.SetName("Buffer is shorter than U64 - mismatching data"),

		new TestCaseData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFC, 0xFD, 0xFE, 0xFF }, new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFC, 0xFD, 0xFE, 0xFF }, null)
			.SetName("Buffer is a multiple of U64 - same data"),

		new TestCaseData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFC, 0xFD, 0xFE, 0xFF }, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x00, 0xFD, 0xFE, 0xFF }, 4)
			.SetName("Buffer is a multiple of U64 - mismatching data"),

		new TestCaseData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF }, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF }, null)
			.SetName("Buffer is longer U64 - same data"),

		new TestCaseData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF }, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0xFC, 0xFD, 0xFE, 0xFF }, 5)
			.SetName("Buffer is longer U64 - mismatching data within U64 alignment"),

		new TestCaseData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF }, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFB, 0xFC, 0xFD, 0xFE, 0xFE }, 9)
			.SetName("Buffer is longer U64 - mismatching data past U64 alignment"),
	];

	[TestCaseSource(nameof(CompareBuffersTestCases))]
	public void TestCompareBuffers(byte[] originalData, byte[] newData, int? expectedFailPosition)
	{
		if (expectedFailPosition.HasValue)
		{
			var exception = Assert.Throws<AssertionException>(CompareCore);

			Assert.That(exception?.Message, Contains.Substring($"at offset {expectedFailPosition}"));
		}
		else
		{
			Assert.DoesNotThrow(CompareCore);
		}

		void CompareCore() => BaseTestFixture.CompareBuffers(originalData, newData);
	}
}