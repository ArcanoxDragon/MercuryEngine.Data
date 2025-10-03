using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public class GenericSectionHeader : DataSectionHeader<GenericSectionHeader>
{
	internal GenericSectionHeader(DataSection parentSection) : base(parentSection) { }

	protected override void Describe(DataStructureBuilder<GenericSectionHeader> builder)
	{
		// Just 100 bytes of padding (to make entire section 144 bytes)
		builder.Padding(0x64);
	}
}