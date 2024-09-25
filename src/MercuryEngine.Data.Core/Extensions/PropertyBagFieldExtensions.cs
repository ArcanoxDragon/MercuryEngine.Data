using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Fields.PropertyBags;

namespace MercuryEngine.Data.Core.Extensions;

[PublicAPI]
public static class PropertyBagFieldExtensions
{
	#region Raw Fields

	public static TField? Get<TField>(this IPropertyBagField propertyBag, string propertyName)
	where TField : IBinaryField
	{
		if (propertyBag.Get(propertyName) is not { } field)
			return default;

		if (field is not TField typedField)
			throw new InvalidOperationException($"Property \"{propertyName}\" has field of type \"{field.GetType().Name}\", which cannot be converted to \"{typeof(TField).Name}\"");

		return typedField;
	}

	public static void SetOrClear(this IPropertyBagField propertyBag, string propertyName, IBinaryField? value)
	{
		if (value is null)
			propertyBag.ClearProperty(propertyName);
		else
			propertyBag.Set(propertyName, value);
	}

	#endregion

	#region Value Fields

	public static void SetOrClearValue<TValue>(this IPropertyBagField propertyBag, string propertyName, TValue? value)
	where TValue : class
	{
		if (value is null)
			propertyBag.ClearProperty(propertyName);
		else
			propertyBag.SetValue(propertyName, value);
	}

	public static void SetOrClearValue<TValue>(this IPropertyBagField propertyBag, string propertyName, TValue? value)
	where TValue : struct
	{
		if (value.HasValue)
			propertyBag.SetValue(propertyName, value.Value);
		else
			propertyBag.ClearProperty(propertyName);
	}

	#endregion

	#region Arrays

	// TODO: Rename ArrayField to ListField or something
	public static IListProperty<T> Array<T>(this IPropertyBagField propertyBag, string propertyName)
	where T : IBinaryField
		=> new PropertyBagListAdapter<T>(propertyBag, propertyName);

	#endregion

	#region Dictionaries

	public static IDictionaryProperty<TKey, TValue> Dictionary<TKey, TValue>(this IPropertyBagField propertyBag, string propertyName)
	where TKey : IBinaryField
	where TValue : IBinaryField
		=> new PropertyBagDictionaryAdapter<TKey, TValue>(propertyBag, propertyName);

	#endregion
}