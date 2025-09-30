using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class StencilState : DataStructure<StencilState>
{
	public bool             Enabled            { get; set; }
	public int              Mask               { get; set; } = 0xFFFFFF;
	public int              Threshold          { get; set; } = -1;
	public StencilOperation FailOperation      { get; set; } = StencilOperation.Zero;
	public StencilOperation PassOperation      { get; set; } = StencilOperation.Zero;
	public StencilOperation DepthFailOperation { get; set; } = StencilOperation.Zero;
	public StencilOperation DepthPassOperation { get; set; } = StencilOperation.Zero;
	public CompareMode      CompareMode        { get; set; } = CompareMode.Always;

	protected override void Describe(DataStructureBuilder<StencilState> builder)
	{
		builder.Property(m => m.Enabled);
		builder.Property(m => m.Mask);
		builder.Property(m => m.Threshold);
		builder.Property(m => m.FailOperation);
		builder.Property(m => m.PassOperation);
		builder.Property(m => m.DepthFailOperation);
		builder.Property(m => m.DepthPassOperation);
		builder.Property(m => m.CompareMode);
	}
}