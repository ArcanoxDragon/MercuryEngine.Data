using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class BlendState : DataStructure<BlendState>
{
	public bool           Enabled              { get; set; }
	public BlendOperation BlendOperation       { get; set; } = BlendOperation.Add;
	public BlendMode      SourceBlendMode      { get; set; } = BlendMode.One;
	public BlendMode      DestinationBlendMode { get; set; } = BlendMode.Zero;

	protected override void Describe(DataStructureBuilder<BlendState> builder)
	{
		builder.Property(m => m.Enabled);
		builder.Property(m => m.BlendOperation);
		builder.Property(m => m.SourceBlendMode);
		builder.Property(m => m.DestinationBlendMode);
	}
}