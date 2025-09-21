using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class ShaderStage : DataStructure<ShaderStage>
{
	public List<UniformParameter> Uniforms { get; } = [];
	public List<Sampler>          Samplers { get; } = [];

	protected override void Describe(DataStructureBuilder<ShaderStage> builder)
	{
		builder.Array(m => m.Uniforms);
		builder.Array(m => m.Samplers);
	}
}