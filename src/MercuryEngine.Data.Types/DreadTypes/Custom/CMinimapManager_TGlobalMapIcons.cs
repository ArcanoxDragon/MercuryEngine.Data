using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Extensions;
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
		public List<GlobalMapIcon> Icons { get; } = [];

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Array(m => m.Icons);
	}

	public sealed class GlobalMapIcon : DataStructure<GlobalMapIcon>
	{
		public string?  IconId  { get; set; }
		public Vector2? IconPos { get; set; }

		protected override void Describe(DataStructureBuilder<GlobalMapIcon> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Property("sIconID", m => m.IconId)
					.RawProperty("vIconPos", m => m.IconPos);
			});
	}
}