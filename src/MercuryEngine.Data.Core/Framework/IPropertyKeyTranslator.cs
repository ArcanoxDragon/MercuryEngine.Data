using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework;

/// <summary>
/// Translates property name strings into keys of type <typeparamref name="TKey"/>.
/// </summary>
[PublicAPI]
public interface IPropertyKeyTranslator<out TKey>
where TKey : notnull
{
	TKey GetEmptyKey();
	TKey TranslateKey(string propertyName);
	uint GetKeySize(string propertyName, uint startPosition);
}