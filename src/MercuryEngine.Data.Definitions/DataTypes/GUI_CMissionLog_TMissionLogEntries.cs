using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Definitions.DataTypes;

public class GUI_CMissionLog_TMissionLogEntries : DataStructure<GUI_CMissionLog_TMissionLogEntries>
{
	public List<DynamicStructure> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<GUI_CMissionLog_TMissionLogEntries> builder)
		=> builder.Array(m => m.Entries, CreateMissionLogEntry);

	private DynamicStructure CreateMissionLogEntry()
		=> DynamicStructure.Create("MissionLogEntry", builder => {
			builder.Enum<EEntryType>("eEntryType")
				   .String("sLabelText")
				   .Array<TerminatedStringDataType>("vCaptionsIds");
		});
}