using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadDataTypeFactories;

public class DreadPointerTypeFactory : BaseDreadDataTypeFactory<DreadPointerType, UInt64DataType>
{
	public static DreadPointerTypeFactory Instance { get; } = new();

	protected override UInt64DataType CreateDataType(DreadPointerType dreadType) => new(); // 64-bit pointers; game is built for an AARCH64 processor
}