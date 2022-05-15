using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Types.DataTypes;

public class StrIdDataType : UInt64DataType
{
	public override string ToString()
		=> BitConverter.GetBytes(Value).ToHexString();
}