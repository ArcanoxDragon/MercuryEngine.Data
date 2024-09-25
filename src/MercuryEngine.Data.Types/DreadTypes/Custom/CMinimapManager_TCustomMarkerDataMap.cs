using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class CMinimapManager_TCustomMarkerDataMap : DataStructure<CMinimapManager_TCustomMarkerDataMap>, ITypedDreadField
{
	public string TypeName => "CMinimapManager::TCustomMarkerDataMap";

	public Dictionary<Int32Field, Entry> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<CMinimapManager_TCustomMarkerDataMap> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : BaseDreadDataStructure<Entry>
	{
		public int MarkerId
		{
			get => RawFields.GetValue<int>("nMarkerId");
			set => RawFields.SetValue("nMarkerId", value);
		}

		public EMarkerType Type
		{
			get => RawFields.GetValue<EMarkerType>("eType");
			set => RawFields.SetValue("eType", value);
		}

		public Vector2? Pos
		{
			get => RawFields.Get<Vector2>("vPos");
			set => RawFields.SetOrClear("vPos", value);
		}

		public string? TargetID
		{
			get => RawFields.GetValue<string>("sTargetID");
			set => RawFields.SetOrClearValue("sTargetID", value);
		}

		public int TargetSlot
		{
			get => RawFields.GetValue<int>("nTargetSlot");
			set => RawFields.SetValue("nTargetSlot", value);
		}

		protected override void DefineFields(PropertyBagFieldBuilder fields)
		{
			fields.Int32("nMarkerId");
			fields.Enum<EMarkerType>("eType");
			fields.AddField<Vector2>("vPos");
			fields.String("sTargetID");
			fields.Int32("nTargetSlot");
		}
	}
}