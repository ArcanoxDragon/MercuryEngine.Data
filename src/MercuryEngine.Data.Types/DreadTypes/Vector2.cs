using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

public class Vector2 : DataStructure<Vector2>, ITypedDreadField
{
	public string TypeName => "base::math::CVector2D";

	public float X { get; set; }
	public float Y { get; set; }

	protected override void Describe(DataStructureBuilder<Vector2> builder)
		=> builder.Property(m => m.X)
			.Property(m => m.Y);
}