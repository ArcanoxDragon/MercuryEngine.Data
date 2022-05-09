using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadPrimitiveType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Primitive;

	public DreadPrimitiveKind PrimitiveKind { get; set; }

	public override IBinaryDataType CreateDataType()
		=> PrimitiveKind switch {
			DreadPrimitiveKind.Bool => new BoolDataType(),
			DreadPrimitiveKind.Int => new Int32DataType(),
			DreadPrimitiveKind.UInt => new UInt32DataType(),
			DreadPrimitiveKind.UInt16 => new UInt16DataType(),
			DreadPrimitiveKind.UInt64 => new UInt64DataType(),
			DreadPrimitiveKind.Float => new FloatDataType(),
			DreadPrimitiveKind.String => new TerminatedStringDataType(),
			// DreadPrimitiveKind.Property => // TODO
			// DreadPrimitiveKind.Bytes => // TODO
			// DreadPrimitiveKind.Float_Vec2 => // TODO
			// DreadPrimitiveKind.Float_Vec3 => // TODO
			// DreadPrimitiveKind.Float_Vec4 => // TODO

			_ => throw new InvalidOperationException($"Unknown or unsupported primitive kind \"{PrimitiveKind}\""),
		};
}