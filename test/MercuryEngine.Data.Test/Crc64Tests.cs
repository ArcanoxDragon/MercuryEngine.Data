using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Test;

public class Crc64Tests
{
	[TestCase("CGameBlackboard")]
	[TestCase("collision_base")]
	[TestCase("TEnabledOccluderCollidersMap")]
	[TestCase("base::global::CRntSmallDictionary<base::global::CStrId, base::spatial::CAABox2D>")]
	[TestCase("base::global::CRntSmallDictionary<base::global::CStrId, bool>")]
	[TestCase("CBreakableTileGroupComponent::TActorTileStatesMap")]
	[TestCase("minimapGrid::TMinimapVisMap")]
	[TestCase("CMinimapManager::TCustomMarkerDataMap")]
	[TestCase("CMinimapManager::TGlobalMapIcons")]
	[TestCase("GUI::CMissionLog::TMissionLogEntries")]
	[TestCase("base::global::CRntVector<EMapTutoType>")]
	public void TestCrc64(string text)
	{
		var crc64 = Crc64.Calculate(text);
		var crc64Hex = crc64.ToHexString();

		TestContext.Out.WriteLine($"\"{text}\" = {crc64Hex}");
	}
}