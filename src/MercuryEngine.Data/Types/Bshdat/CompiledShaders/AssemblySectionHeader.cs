using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bsmat;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public class AssemblySectionHeader : DataSectionHeader<AssemblySectionHeader>
{
	internal AssemblySectionHeader(DataSection parentSection) : base(parentSection) { }

	public ShaderType ShaderType { get; set; }

	protected override void Describe(DataStructureBuilder<AssemblySectionHeader> builder)
	{
		builder.Property(m => m.ShaderType);
		builder.Padding(0x60); // To make entire section 144 bytes
	}
}