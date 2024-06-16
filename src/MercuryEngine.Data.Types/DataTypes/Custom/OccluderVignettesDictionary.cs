using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.DataTypes.Custom;

public class OccluderVignettesDictionary : DataStructure<OccluderVignettesDictionary>, IDreadDataType
{
	public string TypeName => "OccluderVignettesDictionary";

	public Dictionary<TerminatedStringDataType, BoolDataType> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<OccluderVignettesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}