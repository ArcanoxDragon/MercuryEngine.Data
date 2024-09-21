using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

public class CollisionComponentDependencies : ComponentDependencies<CollisionComponentDependencies>
{
	public string File { get; set; } = string.Empty;

	public ushort Unknown1 { get; set; }

	protected override void Describe(DataStructureBuilder<CollisionComponentDependencies> builder)
		=> builder
			.Property(m => m.File)
			.Property(m => m.Unknown1);
}