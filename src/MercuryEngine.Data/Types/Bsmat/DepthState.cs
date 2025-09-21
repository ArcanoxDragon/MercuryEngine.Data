using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class DepthState : DataStructure<DepthState>
{
	public byte        DepthTest        { get; set; }
	public byte        DepthWrite       { get; set; }
	public CompareMode DepthCompareMode { get; set; }
	public byte        ZPrePass         { get; set; }

	protected override void Describe(DataStructureBuilder<DepthState> builder)
	{
		builder.Property(m => m.DepthTest);
		builder.Property(m => m.DepthWrite);
		builder.Property(m => m.DepthCompareMode);
		builder.Property(m => m.ZPrePass);
	}
}