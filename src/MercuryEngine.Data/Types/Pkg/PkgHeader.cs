using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Pkg;

internal class PkgHeader : DataStructure<PkgHeader>
{
	public int               HeaderSize      { get; set; }
	public int               DataSectionSize { get; set; }
	public List<PackageFile> Files           { get; } = [];

	protected override void Describe(DataStructureBuilder<PkgHeader> builder)
	{
		builder.Property(m => m.HeaderSize);
		builder.Property(m => m.DataSectionSize);
		builder.Array(m => m.Files);
	}
}