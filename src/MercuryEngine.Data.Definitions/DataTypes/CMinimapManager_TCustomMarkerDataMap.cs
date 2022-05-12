using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Definitions.DataTypes;

public class CMinimapManager_TCustomMarkerDataMap : DataStructure<CMinimapManager_TCustomMarkerDataMap>
{
	public Dictionary<Int32DataType, DynamicStructure> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<CMinimapManager_TCustomMarkerDataMap> builder)
		=> builder.Dictionary(m => m.Entries, () => new Int32DataType(), CreateEntry);

	private static DynamicStructure CreateEntry()
		=> DynamicStructure.Create("CustomMarkerEntry", builder => {
			builder.Int32("nMarkerId")
				   .Enum<EMarkerType>("eType")
				   .Structure<Vector2>("vPos")
				   .String("sTargetID")
				   .Int32("nTargetSlot");
		});
}