using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bcskla;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Formats;

public class Bcskla : BinaryFormat<Bcskla>
{
	[JsonIgnore]
	public override string DisplayName => "BCSKLA";

	public FileVersion Version    { get; set; } = new(1, 10, 0);
	public int         UnkInt     { get; set; }
	public float       FrameCount { get; set; }

	public List<BoneTrack> Tracks { get; } = [];

	protected override void Describe(DataStructureBuilder<Bcskla> builder)
	{
		builder.Constant("MANM", "<magic>", terminated: false);
		builder.RawProperty(m => m.Version);
		builder.Property(m => m.UnkInt);
		builder.Property(m => m.FrameCount);
		builder.Array(m => m.Tracks, startByteAlignment: 8, paddingByte: 0xFF);
	}
}