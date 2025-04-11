using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadPrimitiveFieldFactory : BaseDreadFieldFactory<DreadPrimitiveType, IBinaryField>
{
	public static DreadPrimitiveFieldFactory Instance { get; } = new();

	protected override IBinaryField CreateField(DreadPrimitiveType dreadType)
		=> dreadType.PrimitiveKind switch {
			DreadPrimitiveKind.Bool       => new BooleanField(),
			DreadPrimitiveKind.Int        => new Int32Field(),
			DreadPrimitiveKind.UInt       => new UInt32Field(),
			DreadPrimitiveKind.UInt16     => new UInt16Field(),
			DreadPrimitiveKind.UInt64     => new UInt64Field(),
			DreadPrimitiveKind.Float      => new FloatField(),
			DreadPrimitiveKind.String     => new TerminatedStringField(),
			DreadPrimitiveKind.Property   => new TerminatedStringField(),
			DreadPrimitiveKind.Bytes      => new DreadPointer<ITypedDreadField>(),
			DreadPrimitiveKind.Float_Vec2 => new Vector2(),
			DreadPrimitiveKind.Float_Vec3 => new Vector3(),
			DreadPrimitiveKind.Float_Vec4 => new Vector4(),

			_ => throw new InvalidOperationException($"Unknown or unsupported primitive kind \"{dreadType.PrimitiveKind}\""),
		};
}