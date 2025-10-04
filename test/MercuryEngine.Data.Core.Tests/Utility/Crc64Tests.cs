using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Tests.Utility;

public class Crc64Tests
{
	[TestCase("CGameBlackboard", ExpectedResult = 0x19C88B27662FBED0ul)]
	[TestCase("collision_base", ExpectedResult = 0x2CCCEF41768CD51Eul)]
	[TestCase("TEnabledOccluderCollidersMap", ExpectedResult = 0x264CB5F3A10C500Cul)]
	[TestCase("base::global::CRntSmallDictionary<base::global::CStrId, base::spatial::CAABox2D>", ExpectedResult = 0xF25C7F4438DE97C8ul)]
	[TestCase("base::global::CRntSmallDictionary<base::global::CStrId, bool>", ExpectedResult = 0x5B7F603B163ADFCAul)]
	[TestCase("CBreakableTileGroupComponent::TActorTileStatesMap", ExpectedResult = 0x76EA98D6172D491Dul)]
	[TestCase("minimapGrid::TMinimapVisMap", ExpectedResult = 0x8D6D71AD17BC9217ul)]
	[TestCase("CMinimapManager::TCustomMarkerDataMap", ExpectedResult = 0x1BEB814E510D45BFul)]
	[TestCase("CMinimapManager::TGlobalMapIcons", ExpectedResult = 0xF9365AAB69F858F0ul)]
	[TestCase("GUI::CMissionLog::TMissionLogEntries", ExpectedResult = 0xD2DCBE71A41CE64Dul)]
	[TestCase("base::global::CRntVector<EMapTutoType>", ExpectedResult = 0x24873FF4B4E3C57Eul)]
	public ulong TestCrc64(string text)
	{
		var crc64 = Crc64.Calculate(text);
		var crc64Hex = crc64.ToHexString();

		TestContext.Out.WriteLine($"\"{text}\" = {crc64Hex} = 0x{crc64:X16}");

		return crc64;
	}
}