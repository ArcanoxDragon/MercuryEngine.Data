using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class AlphaState : DataStructure<AlphaState>
{
	public bool        Enabled     { get; set; }
	public CompareMode CompareMode { get; set; }
	public float       Threshold   { get; set; }

	protected override void Describe(DataStructureBuilder<AlphaState> builder)
	{
		builder.Property(m => m.Enabled);
		builder.Property(m => m.CompareMode);
		builder.Property(m => m.Threshold);
	}
}