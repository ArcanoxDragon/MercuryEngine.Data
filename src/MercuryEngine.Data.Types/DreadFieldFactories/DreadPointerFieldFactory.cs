using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Types.DreadFieldFactories;

public class DreadPointerFieldFactory : BaseDreadFieldFactory<DreadPointerType, UInt64Field>
{
	public static DreadPointerFieldFactory Instance { get; } = new();

	protected override UInt64Field CreateField(DreadPointerType dreadType) => new(); // 64-bit pointers; game is built for an AARCH64 processor
}