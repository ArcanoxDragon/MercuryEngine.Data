using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Types.Fields;

public class StrId : UInt64Field
{
	public override string ToString()
		=> Value.ToHexString();
}