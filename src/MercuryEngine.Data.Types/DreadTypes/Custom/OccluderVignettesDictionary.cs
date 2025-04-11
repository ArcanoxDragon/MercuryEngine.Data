using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class OccluderVignettesDictionary : DataStructure<OccluderVignettesDictionary>, ITypedDreadField
{
	public string TypeName => "base::global::CRntSmallDictionary<base::global::CStrId, bool>";

	public Dictionary<TerminatedStringField, BooleanField> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<OccluderVignettesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}