using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class StorageBuffer(ReflectionSectionHeader reflectionHeader) : ReflectionElement<StorageBuffer>(reflectionHeader)
{
	public uint             Size          { get; private set; }
	public uint             VariableCount { get; private set; }
	public ShaderBindings   Bindings      { get; private set; } = new();
	public ShaderStageFlags Stages        { get; private set; }

	protected override void Describe(DataStructureBuilder<StorageBuffer> builder)
	{
		base.Describe(builder);

		builder.Property(m => m.Size);
		builder.Property(m => m.VariableCount);
		builder.RawProperty(m => m.Bindings);
		builder.Property(m => m.Stages);
		builder.Padding(0x20);
	}
}