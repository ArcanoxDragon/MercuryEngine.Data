using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class GUI_CMissionLog_TMissionLogEntries : DataStructure<GUI_CMissionLog_TMissionLogEntries>, ITypedDreadField
{
	public string TypeName => "GUI::CMissionLog::TMissionLogEntries";

	public List<CGlobalEventManager__SMissionLogEntry> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<GUI_CMissionLog_TMissionLogEntries> builder)
		=> builder.Array(m => m.Entries);
}