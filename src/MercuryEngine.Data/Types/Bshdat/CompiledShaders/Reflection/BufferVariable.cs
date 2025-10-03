using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class BufferVariable(ReflectionSectionHeader reflectionHeader) : ReflectionElement<BufferVariable>(reflectionHeader)
{
	public ShaderValueType  ValueType           { get; private set; }
	public int              BlockIndex          { get; private set; }
	public int              BlockOffset         { get; private set; }
	public uint             ArraySize           { get; private set; }
	public uint             ArrayStride         { get; private set; }
	public int              MatrixStride        { get; private set; }
	public uint             RowMajor            { get; private set; }
	public uint             TopLevelArraySize   { get; private set; }
	public uint             TopLevelArrayStride { get; private set; }
	public ShaderStageFlags Stages              { get; private set; }
	public bool             IsArray             { get; private set; }

	protected override void Describe(DataStructureBuilder<BufferVariable> builder)
	{
		base.Describe(builder);

		builder.Property(m => m.ValueType);
		builder.Property(m => m.BlockIndex);
		builder.Property(m => m.BlockOffset);
		builder.Property(m => m.ArraySize);
		builder.Property(m => m.ArrayStride);
		builder.Property(m => m.MatrixStride);
		builder.Property(m => m.RowMajor);
		builder.Property(m => m.TopLevelArraySize);
		builder.Property(m => m.TopLevelArrayStride);
		builder.Property(m => m.Stages);
		builder.Property(m => m.IsArray);
		builder.Padding(0x1F);
	}
}