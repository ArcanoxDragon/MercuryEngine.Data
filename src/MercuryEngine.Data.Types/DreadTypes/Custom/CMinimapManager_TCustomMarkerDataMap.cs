using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Extensions;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class CMinimapManager_TCustomMarkerDataMap : DataStructure<CMinimapManager_TCustomMarkerDataMap>,
													IDescribeDataStructure<CMinimapManager_TCustomMarkerDataMap>,
													ITypedDreadField
{
	public string TypeName => "CMinimapManager::TCustomMarkerDataMap";

	public Dictionary<Int32Field, Entry> Entries { get; } = [];

	public static void Describe(DataStructureBuilder<CMinimapManager_TCustomMarkerDataMap> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>, IDescribeDataStructure<Entry>
	{
		public int         MarkerId   { get; set; }
		public EMarkerType Type       { get; set; }
		public Vector2?    Pos        { get; set; }
		public string?     TargetID   { get; set; }
		public int         TargetSlot { get; set; }

		public static void Describe(DataStructureBuilder<Entry> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Property("nMarkerId", m => m.MarkerId)
					.Property("eType", m => m.Type)
					.RawProperty("vPos", m => m.Pos)
					.Property("sTargetID", m => m.TargetID)
					.Property("nTargetSlot", m => m.TargetSlot);
			});
	}
}