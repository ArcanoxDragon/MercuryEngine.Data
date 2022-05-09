using MercuryEngine.Data.Framework.DataTypes;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadPointerType : BaseDreadType
{
	public override DreadTypeKind Kind => DreadTypeKind.Pointer;

	public string? Target { get; set; }

	public override IBinaryDataType CreateDataType()
		=> new UInt64DataType(); // 64-bit pointers; game is built for an AARCH64 processor
}