using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat;

public class RenderPass : DataStructure<RenderPass>
{
	public uint Index { get; set; }
	public uint Order { get; set; }

	public ShaderProgramPair? ShaderProgramPair { get; set; }

	protected override void Describe(DataStructureBuilder<RenderPass> builder)
	{
		builder.Property(m => m.Index);
		builder.Property(m => m.Order);
		builder.Pointer(m => m.ShaderProgramPair);
	}
}