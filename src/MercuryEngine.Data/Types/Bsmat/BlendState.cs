using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class BlendState : DataStructure<BlendState>
{
	public bool           Enabled              { get; set; }
	public BlendOperation BlendOperation       { get; set; }
	public BlendMode      SourceBlendMode      { get; set; }
	public BlendMode      DestinationBlendMode { get; set; }

	protected override void Describe(DataStructureBuilder<BlendState> builder)
	{
		builder.Property(m => m.Enabled);
		builder.Property(m => m.BlendOperation);
		builder.Property(m => m.SourceBlendMode);
		builder.Property(m => m.DestinationBlendMode);
	}
}