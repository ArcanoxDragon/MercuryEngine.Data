﻿using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Extensions;
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
		public List<TileState> States { get; } = [];

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Array(m => m.States);
	}

	public sealed class TileState : DataStructure<TileState>
	{
		public float              X        { get; set; }
		public float              Y        { get; set; }
		public EBreakableTileType TileType { get; set; }
		public uint               State    { get; set; }

		protected override void Describe(DataStructureBuilder<TileState> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Property("fX", m => m.X)
					.Property("fY", m => m.Y)
					.Property("eTileType", m => m.TileType)
					.Property("uState", m => m.State);
			});
	}
}