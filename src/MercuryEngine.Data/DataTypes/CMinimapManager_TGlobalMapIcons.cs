using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class CMinimapManager_TGlobalMapIcons : DataStructure<CMinimapManager_TGlobalMapIcons>
{
	public Dictionary<TerminatedStringDataType, ArrayDataType<DynamicStructure>> AreaIcons { get; } = new();

	protected override void Describe(DataStructureBuilder<CMinimapManager_TGlobalMapIcons> builder)
		=> builder.Dictionary(m => m.AreaIcons, () => new TerminatedStringDataType(), () => new ArrayDataType<DynamicStructure>(CreateMapIcon));

	private static DynamicStructure CreateMapIcon()
		=> DynamicStructure.Create("GlobalMapIcon", builder => {
			builder.String("sIconID")
				   .Structure<Vector2>("vIconPos");
		});
}