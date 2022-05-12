using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;

namespace MercuryEngine.Data.Definitions.DataTypes;

public class OccluderVignettesDictionary : DataStructure<OccluderVignettesDictionary>
{
	public Dictionary<TerminatedStringDataType, BoolDataType> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<OccluderVignettesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}