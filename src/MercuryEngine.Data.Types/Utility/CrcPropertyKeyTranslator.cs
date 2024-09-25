using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Utility;

public class CrcPropertyKeyTranslator : IPropertyKeyTranslator<StrId>
{
	public static CrcPropertyKeyTranslator Instance { get; } = new();

	public StrId GetEmptyKey() => new();

	public StrId TranslateKey(string propertyName)
		=> new() { Value = propertyName.GetCrc64() };

	public uint GetKeySize(string propertyName) => sizeof(ulong);
}