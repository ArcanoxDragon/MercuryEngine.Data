using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class OccluderVignettesDictionary : DataStructure<OccluderVignettesDictionary>,
										   IDescribeDataStructure<OccluderVignettesDictionary>,
										   ITypedDreadField
{
	public string TypeName => "OccluderVignettesDictionary";

	public Dictionary<TerminatedStringField, BooleanField> Entries { get; } = [];

	public static void Describe(DataStructureBuilder<OccluderVignettesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}