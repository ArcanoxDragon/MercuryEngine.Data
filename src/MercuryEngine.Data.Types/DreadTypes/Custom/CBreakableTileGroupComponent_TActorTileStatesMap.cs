using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
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
		public List<TileState> States { get; } = [];

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Array(m => m.States);
	}

	public sealed class TileState : BaseDreadDataStructure<TileState>
	{
		public float X
		{
			get => RawFields.GetValue<float>("fX");
			set => RawFields.SetValue("fX", value);
		}

		public float Y
		{
			get => RawFields.GetValue<float>("fY");
			set => RawFields.SetValue("fY", value);
		}

		public EBreakableTileType TileType
		{
			get => RawFields.GetValue<EBreakableTileType>("eTileType");
			set => RawFields.SetValue("eTileType", value);
		}

		public uint State
		{
			get => RawFields.GetValue<uint>("uState");
			set => RawFields.SetValue("uState", value);
		}

		protected override void DefineFields(PropertyBagFieldBuilder fields)
		{
			fields.Float("fX");
			fields.Float("fY");
			fields.Enum<EBreakableTileType>("eTileType");
			fields.UInt32("uState");
		}
	}
}