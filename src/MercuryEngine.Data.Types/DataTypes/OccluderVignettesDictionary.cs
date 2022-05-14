using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.DataTypes;

public class OccluderVignettesDictionary : DataStructure<OccluderVignettesDictionary>
{
	public Dictionary<TerminatedStringDataType, BoolDataType> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<OccluderVignettesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}