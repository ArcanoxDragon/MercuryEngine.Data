using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class UniformBlock(ReflectionSectionHeader reflectionHeader) : ReflectionElement<UniformBlock>(reflectionHeader)
{
	public uint             Size          { get; private set; }
	public uint             VariableCount { get; private set; }
	public ShaderStageFlags Stages        { get; private set; }
	public ShaderBindings   Bindings      { get; private set; } = new();

	protected override void Describe(DataStructureBuilder<UniformBlock> builder)
	{
		base.Describe(builder);

		builder.Property(m => m.Size);
		builder.Property(m => m.VariableCount);
		builder.Property(m => m.Stages);
		builder.RawProperty(m => m.Bindings);
		builder.Padding(0x20);
	}
}