using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class CMinimapManager_TGlobalMapIcons : DataStructure<CMinimapManager_TGlobalMapIcons>,
											   ITypedDreadField
{
	public string TypeName => "CMinimapManager::TGlobalMapIcons";

	public Dictionary<TerminatedStringField, Entry> AreaIcons { get; } = [];

	protected override void Describe(DataStructureBuilder<CMinimapManager_TGlobalMapIcons> builder)
		=> builder.Dictionary(m => m.AreaIcons);

	public sealed class Entry : DataStructure<Entry>
	{
		public List<CMinimapManager__SAreaIconInfo> Icons { get; } = [];

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Array(m => m.Icons);
	}
}