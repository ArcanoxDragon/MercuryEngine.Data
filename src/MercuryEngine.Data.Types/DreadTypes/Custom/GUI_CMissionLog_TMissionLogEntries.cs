using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class GUI_CMissionLog_TMissionLogEntries : DataStructure<GUI_CMissionLog_TMissionLogEntries>, ITypedDreadField
{
	public string TypeName => "GUI::CMissionLog::TMissionLogEntries";

	public List<Entry> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<GUI_CMissionLog_TMissionLogEntries> builder)
		=> builder.Array(m => m.Entries);

	public sealed class Entry : BaseDreadDataStructure<Entry>
	{
		public EEntryType? EntryType
		{
			get => RawFields.GetValue<EEntryType>("eEntryType");
			set => RawFields.SetOrClearValue("eEntryType", value);
		}

		public string? LabelText
		{
			get => RawFields.GetValue<string>("sLabelText");
			set => RawFields.SetOrClearValue("sLabelText", value);
		}

		public IList<TerminatedStringField> CaptionsIds
			=> RawFields.Array<TerminatedStringField>("vCaptionsIds");

		protected override void DefineFields(PropertyBagFieldBuilder fields)
		{
			fields.Enum<EEntryType>("eEntryType");
			fields.String("sLabelText");
			fields.Array<TerminatedStringField>("vCaptionsIds");
		}
	}
}