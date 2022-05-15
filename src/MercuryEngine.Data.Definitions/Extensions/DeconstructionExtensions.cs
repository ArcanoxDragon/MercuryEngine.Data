namespace MercuryEngine.Data.Definitions.Extensions;

internal static class DeconstructionExtensions
{
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
	{
		key = keyValuePair.Key;
		value = keyValuePair.Value;
	}
}