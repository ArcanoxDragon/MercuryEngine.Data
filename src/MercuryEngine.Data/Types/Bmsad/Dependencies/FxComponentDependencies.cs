using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

public class FxComponentDependencies : ComponentDependencies<FxComponentDependencies>
{
	public List<FxDependency> Dependencies { get; } = [];

	protected override void Describe(DataStructureBuilder<FxComponentDependencies> builder)
		=> builder.Array(m => m.Dependencies);

	public sealed class FxDependency : DataStructure<FxDependency>
	{
		public string File { get; set; } = string.Empty;

		public uint Unknown1 { get; set; }
		public uint Unknown2 { get; set; }
		public byte Unknown3 { get; set; }

		protected override void Describe(DataStructureBuilder<FxDependency> builder)
			=> builder
				.Property(m => m.File)
				.Property(m => m.Unknown1)
				.Property(m => m.Unknown2)
				.Property(m => m.Unknown3);
	}
}