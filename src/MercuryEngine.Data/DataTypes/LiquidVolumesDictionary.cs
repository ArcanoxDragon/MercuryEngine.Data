using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class LiquidVolumesDictionary : DataStructure<LiquidVolumesDictionary>
{
	public Dictionary<TerminatedStringDataType, DynamicStructure> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<LiquidVolumesDictionary> builder)
		=> builder.Dictionary(m => m.Entries, () => new TerminatedStringDataType(), CreateEntry);

	private static DynamicStructure CreateEntry()
		=> DynamicStructure.Create("LiquidVolumesDictionaryEntry", builder => {
			builder.Structure<Vector2>("Min")
				   .Structure<Vector2>("Max");
		});
}