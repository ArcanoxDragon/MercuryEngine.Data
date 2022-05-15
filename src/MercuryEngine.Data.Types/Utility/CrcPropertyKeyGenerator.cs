using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Types.DataTypes;

namespace MercuryEngine.Data.Types.Utility;

public class CrcPropertyKeyGenerator : IPropertyKeyGenerator<StrIdDataType>
{
	public static CrcPropertyKeyGenerator Instance { get; } = new();

	public StrIdDataType GenerateKey(string propertyName)
		=> new() { Value = propertyName.GetCrc64() };
}