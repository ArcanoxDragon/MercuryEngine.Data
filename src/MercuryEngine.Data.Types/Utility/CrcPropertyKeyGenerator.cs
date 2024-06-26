using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Utility;

public class CrcPropertyKeyGenerator : IPropertyKeyGenerator<StrId>
{
	public static CrcPropertyKeyGenerator Instance { get; } = new();

	public StrId GetEmptyKey() => new();

	public StrId GenerateKey(string propertyName)
		=> new() { Value = propertyName.GetCrc64() };
}