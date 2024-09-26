using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Structures.Fluent;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="T">The type of structure this <see cref="DataStructureBuilder{T}"/> can build.</typeparam>
[PublicAPI]
public sealed class DataStructureBuilder<T>
where T : IDataStructure
{
	internal DataStructureBuilder() { }

	private List<DataStructureField> Fields { get; init; } = [];

	internal List<DataStructureField> Build() => Fields;

	public DataStructureBuilder<TOther> For<TOther>()
	where TOther : T
		=> new() {
			Fields = Fields, // Build to same field collection
		};

	#region Constant Fields

	public DataStructureBuilder<T> Constant(string text, string? description = null, bool assertValueOnRead = true, bool terminated = true)
	{
		BaseBinaryField<string> field = terminated ? new TerminatedStringField(text) : new FixedLengthStringField(text);
		return AddConstantField(field, description, assertValueOnRead);
	}

	public DataStructureBuilder<T> Constant(bool value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new BooleanField(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(byte value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new ByteField(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(char value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new CharField(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(short value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new Int16Field(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(ushort value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new UInt16Field(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(int value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new Int32Field(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(uint value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new UInt32Field(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(long value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new Int64Field(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(ulong value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new UInt64Field(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(float value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new FloatField(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(double value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new DoubleField(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(decimal value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(new DecimalField(value), description, assertValueOnRead);

	public DataStructureBuilder<T> Constant<TEnum>(TEnum value, string? description = null, bool assertValueOnRead = true)
	where TEnum : struct, Enum
		=> AddConstantField(new EnumField<TEnum>(value), description, assertValueOnRead);

	public DataStructureBuilder<T> AddConstantField<TValue>(IBinaryField<TValue> field, string? description = null, bool assertValueOnRead = true)
	where TValue : notnull
		=> AddField(new DataStructureField(new ConstantValueFieldHandler<TValue>(field, assertValueOnRead), description ?? $"<constant: {field}>"));

	#endregion

	#region Property Fields

	public DataStructureBuilder<T> Property(Expression<Func<T, string?>> propertyExpression)
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var nullabilityInfo = ReflectionUtility.NullabilityInfoContext.Create(property);

		if (nullabilityInfo.WriteState == NullabilityState.NotNull)
			// Property is annotated as non-null, so we will use Property instead of NullableProperty
			// to ensure that "null" is not written during reading
			return Property(propertyExpression!, new TerminatedStringField());

		return NullableProperty(propertyExpression, new TerminatedStringField());
	}

	public DataStructureBuilder<T> Property(Expression<Func<T, bool>> propertyExpression)
		=> Property(propertyExpression, new BooleanField());

	public DataStructureBuilder<T> Property(Expression<Func<T, bool?>> propertyExpression)
		=> NullableProperty(propertyExpression, new BooleanField());

	public DataStructureBuilder<T> Property(Expression<Func<T, byte>> propertyExpression)
		=> Property(propertyExpression, new ByteField());

	public DataStructureBuilder<T> Property(Expression<Func<T, byte?>> propertyExpression)
		=> NullableProperty(propertyExpression, new ByteField());

	public DataStructureBuilder<T> Property(Expression<Func<T, char>> propertyExpression)
		=> Property(propertyExpression, new CharField());

	public DataStructureBuilder<T> Property(Expression<Func<T, char?>> propertyExpression)
		=> NullableProperty(propertyExpression, new CharField());

	public DataStructureBuilder<T> Property(Expression<Func<T, short>> propertyExpression)
		=> Property(propertyExpression, new Int16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, short?>> propertyExpression)
		=> NullableProperty(propertyExpression, new Int16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort>> propertyExpression)
		=> Property(propertyExpression, new UInt16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort?>> propertyExpression)
		=> NullableProperty(propertyExpression, new UInt16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, int>> propertyExpression)
		=> Property(propertyExpression, new Int32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, int?>> propertyExpression)
		=> NullableProperty(propertyExpression, new Int32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, uint>> propertyExpression)
		=> Property(propertyExpression, new UInt32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, uint?>> propertyExpression)
		=> NullableProperty(propertyExpression, new UInt32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, long>> propertyExpression)
		=> Property(propertyExpression, new Int64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, long?>> propertyExpression)
		=> NullableProperty(propertyExpression, new Int64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong>> propertyExpression)
		=> Property(propertyExpression, new UInt64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong?>> propertyExpression)
		=> NullableProperty(propertyExpression, new UInt64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, float>> propertyExpression)
		=> Property(propertyExpression, new FloatField());

	public DataStructureBuilder<T> Property(Expression<Func<T, float?>> propertyExpression)
		=> NullableProperty(propertyExpression, new FloatField());

	public DataStructureBuilder<T> Property(Expression<Func<T, double>> propertyExpression)
		=> Property(propertyExpression, new DoubleField());

	public DataStructureBuilder<T> Property(Expression<Func<T, double?>> propertyExpression)
		=> NullableProperty(propertyExpression, new DoubleField());

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal>> propertyExpression)
		=> Property(propertyExpression, new DecimalField());

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal?>> propertyExpression)
		=> NullableProperty(propertyExpression, new DecimalField());

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> Property(propertyExpression, new EnumField<TEnum>());

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> NullableProperty(propertyExpression, new EnumField<TEnum>());

	public DataStructureBuilder<T> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression, IBinaryField<TProperty> field)
	where TProperty : notnull
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<TProperty>(field, property);
		var description = $"{property.Name}: {typeof(TProperty).Name}";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> NullableProperty<TProperty>(Expression<Func<T, TProperty?>> propertyExpression, IBinaryField<TProperty> field)
	where TProperty : class
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<TProperty>(field, property, nullable: true);
		var description = $"{property.Name}: {typeof(TProperty).Name}?";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> NullableProperty<TProperty>(Expression<Func<T, TProperty?>> propertyExpression, IBinaryField<TProperty> field)
	where TProperty : struct
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<TProperty>(field, property, nullable: true);
		var description = $"{property.Name}: {typeof(TProperty).Name}?";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	#region Raw Properties

	public DataStructureBuilder<T> RawProperty<TField>(Expression<Func<T, TField>> propertyExpression)
	where TField : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new DirectPropertyFieldHandler(property);
		var description = $"{property.Name}: {typeof(TField).Name}";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> NullableRawProperty<TField>(Expression<Func<T, TField?>> propertyExpression)
	where TField : class, IBinaryField, new()
		=> NullableRawProperty(propertyExpression, () => new TField());

	public DataStructureBuilder<T> NullableRawProperty<TField>(Expression<Func<T, TField?>> propertyExpression, Func<TField> fieldFactory)
	where TField : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new NullableDirectPropertyFieldHandler<TField>(property, fieldFactory);
		var description = $"{property.Name}: {typeof(TField).Name}?";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	#region Collection Properties

	public DataStructureBuilder<T> Array<TItem>(Expression<Func<T, IList<TItem>>> propertyExpression)
	where TItem : class, IBinaryField, new()
		=> Array(propertyExpression, () => new TItem());

	public DataStructureBuilder<T> Array<TItem>(Expression<Func<T, IList<TItem>>> propertyExpression, Func<TItem> itemFactory)
	where TItem : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var field = new ArrayField<TItem>(itemFactory);
		var adapter = new ArrayPropertyFieldHandler<TItem>(field, property);
		var description = $"{property.Name}: {typeof(TItem).Name}[]";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression)
	where TKey : class, IBinaryField, new()
	where TValue : class, IBinaryField, new()
		=> Dictionary(propertyExpression, () => new TKey(), () => new TValue());

	public DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryField
	where TValue : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var field = new DictionaryField<TKey, TValue>(keyFactory, valueFactory);
		var adapter = new DictionaryPropertyFieldHandler<TKey, TValue>(field, property);
		var description = $"{property.Name}: Dictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	public DataStructureBuilder<T> AddField(DataStructureField field)
	{
		Fields.Add(field);
		return this;
	}
}