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
			DreadPrimitiveKind.Bool       => new DreadNumberField<bool, BooleanField>(dreadType.TypeName),
			DreadPrimitiveKind.Int        => new DreadNumberField<int, Int32Field>(dreadType.TypeName),
			DreadPrimitiveKind.UInt       => new DreadNumberField<uint, UInt32Field>(dreadType.TypeName),
			DreadPrimitiveKind.UInt16     => new DreadNumberField<ushort, UInt16Field>(dreadType.TypeName),
			DreadPrimitiveKind.UInt64     => new DreadNumberField<ulong, UInt64Field>(dreadType.TypeName),
			DreadPrimitiveKind.Float      => new DreadNumberField<float, FloatField>(dreadType.TypeName),
			DreadPrimitiveKind.String     => new DreadStringField(dreadType.TypeName),
			DreadPrimitiveKind.Property   => new TerminatedStringField(),
			DreadPrimitiveKind.Bytes      => new DreadPointer<ITypedDreadField>(),
			DreadPrimitiveKind.Float_Vec2 => new Vector2(),
			DreadPrimitiveKind.Float_Vec3 => new Vector3(),
			DreadPrimitiveKind.Float_Vec4 => new Vector4(),

			_ => throw new InvalidOperationException($"Unknown or unsupported primitive kind \"{dreadType.PrimitiveKind}\""),
		};
}