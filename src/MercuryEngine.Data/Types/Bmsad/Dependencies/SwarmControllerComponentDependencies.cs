using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

public class SwarmControllerComponentDependencies : ComponentDependencies<SwarmControllerComponentDependencies>
{
	// TODO: List Adapters

	public List<TerminatedStringField> Unknown1 { get; } = [];
	public List<TerminatedStringField> Unknown2 { get; } = [];
	public List<TerminatedStringField> Unknown3 { get; } = [];

	protected override void Describe(DataStructureBuilder<SwarmControllerComponentDependencies> builder)
		=> builder
			.Array(m => m.Unknown1)
			.Array(m => m.Unknown2)
			.Array(m => m.Unknown3);
}