using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class OccluderVignettesDictionary : DataStructure<OccluderVignettesDictionary>
{
	public Dictionary<TerminatedStringDataType, BoolDataType> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<OccluderVignettesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}