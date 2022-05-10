using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class CBreakableTileGroupComponent_TActorTileStatesMap : DataStructure<CBreakableTileGroupComponent_TActorTileStatesMap>
{
	public Dictionary<TerminatedStringDataType, Entry> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<CBreakableTileGroupComponent_TActorTileStatesMap> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public List<DynamicStructure> States { get; } = new();

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Array(m => m.States, CreateState);

		private static DynamicStructure CreateState()
			=> DynamicStructure.Create("ActorTileState", builder => {
				builder.Float("fX")
					   .Float("fY")
					   .Enum<EBreakableTileType>("eTileType")
					   .UInt32("uState");
			});
	}
}