using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class ShaderInput(ReflectionSectionHeader reflectionHeader) : ReflectionElement<ShaderInput>(reflectionHeader)
{
	public ShaderValueType  ValueType  { get; private set; }
	public uint             ArraySize  { get; private set; }
	public int              Location   { get; private set; }
	public ShaderStageFlags Stages     { get; private set; }
	public bool             IsArray    { get; private set; }
	public bool             IsPerPatch { get; private set; }

	protected override void Describe(DataStructureBuilder<ShaderInput> builder)
	{
		base.Describe(builder);

		builder.Property(m => m.ValueType);
		builder.Property(m => m.ArraySize);
		builder.Property(m => m.Location);
		builder.Property(m => m.Stages);
		builder.Property(m => m.IsArray);
		builder.Property(m => m.IsPerPatch);
		builder.Padding(0x22);
	}
}