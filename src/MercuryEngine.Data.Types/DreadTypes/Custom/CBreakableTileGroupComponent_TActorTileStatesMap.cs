using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class CBreakableTileGroupComponent_TActorTileStatesMap : DataStructure<CBreakableTileGroupComponent_TActorTileStatesMap>, ITypedDreadField
{
	public string TypeName => "CBreakableTileGroupComponent::TActorTileStatesMap";

	public Dictionary<TerminatedStringField, Entry> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<CBreakableTileGroupComponent_TActorTileStatesMap> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public List<CBreakableTileGroupComponent__SMinimapTileState> States { get; } = [];

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Array(m => m.States);
	}
}