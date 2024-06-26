using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Extensions;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class GUI_CMissionLog_TMissionLogEntries : DataStructure<GUI_CMissionLog_TMissionLogEntries>, ITypedDreadField
{
	public string TypeName => "GUI::CMissionLog::TMissionLogEntries";

	public List<Entry> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<GUI_CMissionLog_TMissionLogEntries> builder)
		=> builder.Array(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public EEntryType                   EntryType   { get; set; }
		public string?                      LabelText   { get; set; }
		public List<TerminatedStringField>? CaptionsIds { get; set; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Property("eEntryType", m => m.EntryType)
					.Property("sLabelText", m => m.LabelText)
					.Array("vCaptionsIds", m => m.CaptionsIds);
			});
	}
}