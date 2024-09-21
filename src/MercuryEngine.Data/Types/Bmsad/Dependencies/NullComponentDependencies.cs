using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

public sealed class NullComponentDependencies : ComponentDependencies
{
	protected override void Describe(DataStructureBuilder<ComponentDependencies> builder) { }
}