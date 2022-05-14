using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.DataTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadPrimitiveTypeFactory : BaseDreadDataTypeFactory<DreadPrimitiveType, IBinaryDataType>
{
	public static DreadPrimitiveTypeFactory Instance { get; } = new();

	protected override IBinaryDataType CreateDataType(DreadPrimitiveType dreadType)
		=> dreadType.PrimitiveKind switch {
			DreadPrimitiveKind.Bool => new BoolDataType(),
			DreadPrimitiveKind.Int => new Int32DataType(),
			DreadPrimitiveKind.UInt => new UInt32DataType(),
			DreadPrimitiveKind.UInt16 => new UInt16DataType(),
			DreadPrimitiveKind.UInt64 => new UInt64DataType(),
			DreadPrimitiveKind.Float => new FloatDataType(),
			DreadPrimitiveKind.String => new TerminatedStringDataType(),
			// DreadPrimitiveKind.Property => // TODO
			// DreadPrimitiveKind.Bytes => // TODO
			DreadPrimitiveKind.Float_Vec2 => new Vector2(),
			DreadPrimitiveKind.Float_Vec3 => new Vector3(),
			DreadPrimitiveKind.Float_Vec4 => new Vector4(),

			_ => throw new InvalidOperationException($"Unknown or unsupported primitive kind \"{dreadType.PrimitiveKind}\""),
		};
}