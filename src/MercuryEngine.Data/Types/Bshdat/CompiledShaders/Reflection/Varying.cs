using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class Varying(ReflectionSectionHeader reflectionHeader) : ReflectionElement<Varying>(reflectionHeader)
{
	public ShaderValueType ValueType { get; private set; }
	public uint            ArraySize { get; private set; }
	public bool            IsArray   { get; private set; }

	protected override void Describe(DataStructureBuilder<Varying> builder)
	{
		base.Describe(builder);

		builder.Property(m => m.ValueType);
		builder.Property(m => m.ArraySize);
		builder.Property(m => m.IsArray);
		builder.Padding(0x1F);
	}
}