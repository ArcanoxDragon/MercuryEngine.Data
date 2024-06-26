namespace MercuryEngine.Data.Core.Framework;

/// <summary>
/// Generates property keys of type <typeparamref name="TKey"/> from property names.
/// </summary>
public interface IPropertyKeyGenerator<out TKey>
where TKey : notnull
{
	TKey GetEmptyKey();
	TKey GenerateKey(string propertyName);
}