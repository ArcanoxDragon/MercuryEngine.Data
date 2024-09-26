using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class LiquidVolumesDictionary : DataStructure<LiquidVolumesDictionary>, ITypedDreadField
{
	public string TypeName => "LiquidVolumesDictionary";

	public Dictionary<TerminatedStringField, base__spatial__CAABox2D> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<LiquidVolumesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);
}