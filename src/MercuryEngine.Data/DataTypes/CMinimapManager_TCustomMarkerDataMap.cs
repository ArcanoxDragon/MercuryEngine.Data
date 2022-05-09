using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class CMinimapManager_TCustomMarkerDataMap : DataStructure<CMinimapManager_TCustomMarkerDataMap>
{
	public Dictionary<Int32DataType, Entry> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<CMinimapManager_TCustomMarkerDataMap> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public int         Id         { get; set; }
		public EMarkerType Type       { get; set; }
		public Vector2     Position   { get; set; } = new();
		public string      TargetId   { get; set; } = string.Empty;
		public int         TargetSlot { get; set; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			// TODO: Proper property maps
			=> builder.Int32(5)
					  .CrcLiteral("nMarkerId")
					  .Int32(m => m.Id)
					  .CrcLiteral("eType")
					  .Enum(m => m.Type)
					  .CrcLiteral("vPos")
					  .Structure(m => m.Position)
					  .CrcLiteral("sTargetID")
					  .String(m => m.TargetId)
					  .CrcLiteral("nTargetSlot")
					  .Int32(m => m.TargetSlot);
	}
}