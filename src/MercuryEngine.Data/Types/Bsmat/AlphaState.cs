using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class AlphaState : DataStructure<AlphaState>
{
	public bool        Enabled     { get; set; }
	public CompareMode CompareMode { get; set; } = CompareMode.Greater;
	public float       Threshold   { get; set; } = 0.5f;

	protected override void Describe(DataStructureBuilder<AlphaState> builder)
	{
		builder.Property(m => m.Enabled);
		builder.Property(m => m.CompareMode);
		builder.Property(m => m.Threshold);
	}
}