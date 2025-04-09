using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

[JsonDerivedType(typeof(BillboardComponentDependencies))]
[JsonDerivedType(typeof(CollisionComponentDependencies))]
[JsonDerivedType(typeof(FxComponentDependencies))]
[JsonDerivedType(typeof(GrabComponentDependencies))]
[JsonDerivedType(typeof(NullComponentDependencies))]
[JsonDerivedType(typeof(SwarmControllerComponentDependencies))]
public abstract class ComponentDependencies : DataStructure<ComponentDependencies>
{
	internal static readonly string[] KnownComponentDependencyTypes = [
		"CFXComponent",
		"CStandaloneFXComponent",
		"CCollisionComponent",
		"CGrabComponent",
		"CBillboardComponent",
		"CSwarmControllerComponent",
	];

	public static SwitchField<ComponentDependencies> Create(ActorDefComponent component)
		=> SwitchField<ComponentDependencies>.FromProperty(component, c => c.TypeForDependencies, builder => {
			builder.AddCase("CFXComponent", new FxComponentDependencies());
			builder.AddCase("CStandaloneFXComponent", new FxComponentDependencies());
			builder.AddCase("CCollisionComponent", new CollisionComponentDependencies());
			builder.AddCase("CGrabComponent", new GrabComponentDependencies());
			builder.AddCase("CBillboardComponent", new BillboardComponentDependencies());
			builder.AddCase("CSwarmControllerComponent", new SwarmControllerComponentDependencies());
			builder.AddFallback(new NullComponentDependencies());
		});
}

public abstract class ComponentDependencies<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TSelf
> : ComponentDependencies
where TSelf : ComponentDependencies
{
	protected sealed override void Describe(DataStructureBuilder<ComponentDependencies> builder)
		=> Describe(builder.For<TSelf>());

	protected abstract void Describe(DataStructureBuilder<TSelf> builder);
}