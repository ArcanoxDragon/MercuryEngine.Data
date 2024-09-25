namespace MercuryEngine.Data.Core.Framework;

/// <summary>
/// Translates property name strings into keys of type <typeparamref name="TKey"/>.
/// </summary>
public interface IPropertyKeyTranslator<out TKey>
where TKey : notnull
{
	TKey GetEmptyKey();
	TKey TranslateKey(string propertyName);
	uint GetKeySize(string propertyName);
}