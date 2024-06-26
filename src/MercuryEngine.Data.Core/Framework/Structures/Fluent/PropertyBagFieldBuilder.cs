using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Structures.Fluent;

[PublicAPI]
public sealed class PropertyBagFieldBuilder<T>
where T : IDataStructure
{
	private static readonly NullabilityInfoContext NullabilityInfoContext = new();

	internal PropertyBagFieldBuilder(T structure)
	{
		BuildingStructure = structure;
	}

	private T BuildingStructure { get; }

	internal Dictionary<string, DataStructureField> Fields { get; } = [];

	#region Property Fields

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, string?>> propertyExpression)
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var nullabilityInfo = NullabilityInfoContext.Create(property);

		if (nullabilityInfo.WriteState == NullabilityState.NotNull)
			// Property is annotated as non-null, so we will use Property instead of NullableProperty
			// to ensure that "null" is not written during reading
			return Property(propertyKey, propertyExpression!, new TerminatedStringField());

		return NullableProperty(propertyKey, propertyExpression, new TerminatedStringField());
	}

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, bool>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new BooleanField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, bool?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new BooleanField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, short>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new Int16Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, short?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new Int16Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, ushort>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new UInt16Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, ushort?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new UInt16Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, int>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new Int32Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, int?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new Int32Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, uint>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new UInt32Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, uint?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new UInt32Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, long>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new Int64Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, long?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new Int64Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, ulong>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new UInt64Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, ulong?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new UInt64Field());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, float>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new FloatField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, float?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new FloatField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, double>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new DoubleField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, double?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new DoubleField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, decimal>> propertyExpression)
		=> Property(propertyKey, propertyExpression, new DecimalField());

	public PropertyBagFieldBuilder<T> Property(string propertyKey, Expression<Func<T, decimal?>> propertyExpression)
		=> NullableProperty(propertyKey, propertyExpression, new DecimalField());

	public PropertyBagFieldBuilder<T> Property<TEnum>(string propertyKey, Expression<Func<T, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> Property(propertyKey, propertyExpression, new EnumField<TEnum>());

	public PropertyBagFieldBuilder<T> Property<TEnum>(string propertyKey, Expression<Func<T, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> NullableProperty(propertyKey, propertyExpression, new EnumField<TEnum>());

	public PropertyBagFieldBuilder<T> Property<TProperty>(string propertyKey, Expression<Func<T, TProperty>> propertyExpression, IBinaryField<TProperty> field)
	where TProperty : notnull
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<TProperty>(field, BuildingStructure, property);
		var description = $"{propertyKey} -> {property.Name}: {typeof(TProperty).Name}";
		return AddField(propertyKey, new DataStructureField(adapter, description));
	}

	public PropertyBagFieldBuilder<T> NullableProperty<TProperty>(string propertyKey, Expression<Func<T, TProperty?>> propertyExpression, IBinaryField<TProperty> field)
	where TProperty : class
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<TProperty>(field, BuildingStructure, property, nullable: true);
		var description = $"{propertyKey} -> {property.Name}: {typeof(TProperty).Name}?";
		return AddField(propertyKey, new DataStructureField(adapter, description));
	}

	public PropertyBagFieldBuilder<T> NullableProperty<TProperty>(string propertyKey, Expression<Func<T, TProperty?>> propertyExpression, IBinaryField<TProperty> field)
	where TProperty : struct
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<TProperty>(field, BuildingStructure, property, nullable: true);
		var description = $"{propertyKey} -> {property.Name}: {typeof(TProperty).Name}?";
		return AddField(propertyKey, new DataStructureField(adapter, description));
	}

	#endregion

	#region Direct Properties

	public PropertyBagFieldBuilder<T> RawProperty<TField>(string propertyKey, Expression<Func<T, TField?>> propertyExpression)
	where TField : class, IBinaryField, new()
		=> RawProperty(propertyKey, propertyExpression, () => new TField());

	public PropertyBagFieldBuilder<T> RawProperty<TField>(string propertyKey, Expression<Func<T, TField?>> propertyExpression, Func<TField> fieldFactory)
	where TField : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new NullableDirectPropertyFieldHandler(BuildingStructure, property, fieldFactory);
		var description = $"{propertyKey} -> {property.Name}: {typeof(TField).Name}";
		return AddField(propertyKey, new DataStructureField(adapter, description));
	}

	#endregion

	#region Collection Properties

	public PropertyBagFieldBuilder<T> Array<TItem>(string propertyKey, Expression<Func<T, IList<TItem>?>> propertyExpression)
	where TItem : class, IBinaryField, new()
		=> Array(propertyKey, propertyExpression, () => new TItem());

	public PropertyBagFieldBuilder<T> Array<TItem>(string propertyKey, Expression<Func<T, IList<TItem>?>> propertyExpression, Func<TItem> itemFactory)
	where TItem : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var field = new ArrayField<TItem>(itemFactory);
		var adapter = new ArrayPropertyFieldHandler<TItem>(field, BuildingStructure, property, activateWhenNull: true);
		var description = $"{propertyKey} -> {property.Name}: {typeof(TItem).Name}[]";
		return AddField(propertyKey, new DataStructureField(adapter, description));
	}

	public PropertyBagFieldBuilder<T> Dictionary<TKey, TValue>(string propertyKey, Expression<Func<T, IDictionary<TKey, TValue>?>> propertyExpression)
	where TKey : class, IBinaryField, new()
	where TValue : class, IBinaryField, new()
		=> Dictionary(propertyKey, propertyExpression, () => new TKey(), () => new TValue());

	public PropertyBagFieldBuilder<T> Dictionary<TKey, TValue>(string propertyKey, Expression<Func<T, IDictionary<TKey, TValue>?>> propertyExpression, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryField
	where TValue : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var field = new DictionaryField<TKey, TValue>(keyFactory, valueFactory);
		var adapter = new DictionaryPropertyFieldHandler<TKey, TValue>(field, BuildingStructure, property, activateWhenNull: true);
		var description = $"{propertyKey} -> {property.Name}: Dictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";
		return AddField(propertyKey, new DataStructureField(adapter, description));
	}

	#endregion

	public PropertyBagFieldBuilder<T> AddField(string propertyKey, DataStructureField field)
	{
		if (!Fields.TryAdd(propertyKey, field))
			throw new InvalidOperationException($"A property with the key \"{propertyKey}\" has already been defined");

		return this;
	}
}