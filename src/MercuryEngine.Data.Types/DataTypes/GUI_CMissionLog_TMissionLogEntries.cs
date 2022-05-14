using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Extensions;

namespace MercuryEngine.Data.Types.DataTypes;

public class GUI_CMissionLog_TMissionLogEntries : DataStructure<GUI_CMissionLog_TMissionLogEntries>
{
	public List<Entry> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<GUI_CMissionLog_TMissionLogEntries> builder)
		=> builder.Array(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public EEntryType EntryType { get; set; }
		public string? LabelText { get; set; }
		public List<TerminatedStringDataType>? CaptionsIds { get; set; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Property("eEntryType", m => m.EntryType)
					  .Property("sLabelText", m => m.LabelText)
					  .Array("vCaptionsIds", m => m.CaptionsIds);
			});
	}
}