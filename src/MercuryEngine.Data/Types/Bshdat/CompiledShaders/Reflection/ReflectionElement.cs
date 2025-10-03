using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders.Reflection;

public class ReflectionElement<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TSelf
>(ReflectionSectionHeader reflectionHeader) : DataStructure<TSelf>
where TSelf : ReflectionElement<TSelf>
{
	public string Name => NameField.Name;

	#region Private Data

	private ReflectionName NameField { get; } = new(reflectionHeader);

	#endregion

	protected override void Describe(DataStructureBuilder<TSelf> builder)
	{
		builder.RawProperty(m => m.NameField);
	}
}