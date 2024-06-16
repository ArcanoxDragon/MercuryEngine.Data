﻿using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Extensions;

namespace MercuryEngine.Data.Types.DataTypes.Custom;

public class CMinimapManager_TGlobalMapIcons : DataStructure<CMinimapManager_TGlobalMapIcons>, IDreadDataType
{
	public string TypeName => "CMinimapManager::TGlobalMapIcons";

	public Dictionary<TerminatedStringDataType, Entry> AreaIcons { get; } = [];

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
					  .Structure("vIconPos", m => m.IconPos);
			});
	}
}