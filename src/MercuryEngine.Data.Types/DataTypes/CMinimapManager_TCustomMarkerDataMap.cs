using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Extensions;

namespace MercuryEngine.Data.Types.DataTypes;

public class CMinimapManager_TCustomMarkerDataMap : DataStructure<CMinimapManager_TCustomMarkerDataMap>
{
	public Dictionary<Int32DataType, Entry> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<CMinimapManager_TCustomMarkerDataMap> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public int         MarkerId   { get; set; }
		public EMarkerType Type       { get; set; }
		public Vector2?    Pos        { get; set; }
		public string?     TargetID   { get; set; }
		public int         TargetSlot { get; set; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Property("nMarkerId", m => m.MarkerId)
					  .Property("eType", m => m.Type)
					  .Structure("vPos", m => m.Pos)
					  .Property("sTargetID", m => m.TargetID)
					  .Property("nTargetSlot", m => m.TargetSlot);
			});
	}
}