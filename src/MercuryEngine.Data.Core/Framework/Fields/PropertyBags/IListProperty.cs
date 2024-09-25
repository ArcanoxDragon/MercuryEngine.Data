using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.Fields.PropertyBags;

[PublicAPI]
public interface IListProperty<T> : IList<T>
{
	/// <summary>
	/// Removes the list's property entirely so that it is not written with the containing data structure
	/// (as opposed to an empty list being written).
	/// </summary>
	void ClearProperty();
}