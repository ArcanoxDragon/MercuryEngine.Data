using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework;

public class CrcPropertyKeyGenerator : IPropertyKeyGenerator<UInt64DataType>
{
	public static CrcPropertyKeyGenerator Instance { get; } = new();

	public UInt64DataType GenerateKey(string propertyName)
		=> new() { Value = propertyName.GetCrc64() };
}