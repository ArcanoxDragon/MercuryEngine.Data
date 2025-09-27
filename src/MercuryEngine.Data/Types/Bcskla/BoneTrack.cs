using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Bcskla;

public class BoneTrack : DataStructure<BoneTrack>
{
	public StrId           BoneName { get; set; } = new();
	public BoneTrackValues Values   { get; }      = new();

	protected override void Describe(DataStructureBuilder<BoneTrack> builder)
	{
		builder.RawProperty(m => m.BoneName);
		builder.RawProperty(m => m.Values);
	}
}