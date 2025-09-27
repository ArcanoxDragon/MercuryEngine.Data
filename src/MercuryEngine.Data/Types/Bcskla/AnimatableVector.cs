using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcskla;

public class AnimatableVector : DataStructure<AnimatableVector>
{
	public AnimatableValue X { get; } = new();
	public AnimatableValue Y { get; } = new();
	public AnimatableValue Z { get; } = new();

	protected override void Describe(DataStructureBuilder<AnimatableVector> builder)
	{
		builder.RawProperty(m => m.X);
		builder.RawProperty(m => m.Y);
		builder.RawProperty(m => m.Z);
	}
}