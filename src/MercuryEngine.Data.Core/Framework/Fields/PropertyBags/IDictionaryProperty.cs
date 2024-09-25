using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.Fields.PropertyBags;

[PublicAPI]
public interface IDictionaryProperty<TKey, TValue> : IDictionary<TKey, TValue>
{
	/// <summary>
	/// Removes the dictionary's property entirely so that it is not written with the containing data structure
	/// (as opposed to an empty dictionary being written).
	/// </summary>
	void ClearProperty();
}