using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fluent;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="T">The type of structure this <see cref="DataStructureBuilder{T}"/> can build.</typeparam>
[PublicAPI]
public abstract class DataStructureBuilder<T>
where T : IDataStructure
{
	private protected DataStructureBuilder() { }

	protected List<IDataStructureField<T>> Fields { get; } = [];

	#region String Literals

	public DataStructureBuilder<T> Literal(string text, string? description = null)
		=> AddVirtualField(new TerminatedStringField(text), description);

	#endregion

	#region Numeric Literals

	public DataStructureBuilder<T> Literal(bool value, string? description = null)
		=> AddVirtualField(new BooleanField { Value = value }, description);

	public DataStructureBuilder<T> Literal(short value, string? description = null)
		=> AddVirtualField(new Int16Field { Value = value }, description);

	public DataStructureBuilder<T> Literal(ushort value, string? description = null)
		=> AddVirtualField(new UInt16Field { Value = value }, description);

	public DataStructureBuilder<T> Literal(int value, string? description = null)
		=> AddVirtualField(new Int32Field { Value = value }, description);

	public DataStructureBuilder<T> Literal(uint value, string? description = null)
		=> AddVirtualField(new UInt32Field { Value = value }, description);

	public DataStructureBuilder<T> Literal(long value, string? description = null)
		=> AddVirtualField(new Int64Field { Value = value }, description);

	public DataStructureBuilder<T> Literal(ulong value, string? description = null)
		=> AddVirtualField(new UInt64Field { Value = value }, description);

	public DataStructureBuilder<T> Literal(float value, string? description = null)
		=> AddVirtualField(new FloatField { Value = value }, description);

	public DataStructureBuilder<T> Literal(double value, string? description = null)
		=> AddVirtualField(new DoubleField { Value = value }, description);

	public DataStructureBuilder<T> Literal(decimal value, string? description = null)
		=> AddVirtualField(new DecimalField { Value = value }, description);

	public DataStructureBuilder<T> Literal<TEnum>(TEnum value, string? description = null)
	where TEnum : struct, Enum
		=> AddVirtualField(new EnumField<TEnum> { Value = value }, description);

	#endregion

	#region String Properties

	public DataStructureBuilder<T> Property(Expression<Func<T, string?>> propertyExpression)
		=> AddPropertyField<string, TerminatedStringField>(propertyExpression);

	#endregion

	#region Numeric Properties

	public DataStructureBuilder<T> Property(Expression<Func<T, bool>> propertyExpression)
		=> AddPropertyField<bool, BooleanField>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, short>> propertyExpression)
		=> AddPropertyField<short, Int16Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, short?>> propertyExpression)
		=> AddPropertyField<short, Int16Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort>> propertyExpression)
		=> AddPropertyField<ushort, UInt16Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort?>> propertyExpression)
		=> AddPropertyField<ushort, UInt16Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, int>> propertyExpression)
		=> AddPropertyField<int, Int32Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, int?>> propertyExpression)
		=> AddPropertyField<int, Int32Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, uint>> propertyExpression)
		=> AddPropertyField<uint, UInt32Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, uint?>> propertyExpression)
		=> AddPropertyField<uint, UInt32Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, long>> propertyExpression)
		=> AddPropertyField<long, Int64Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, long?>> propertyExpression)
		=> AddPropertyField<long, Int64Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong>> propertyExpression)
		=> AddPropertyField<ulong, UInt64Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong?>> propertyExpression)
		=> AddPropertyField<ulong, UInt64Field>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, float>> propertyExpression)
		=> AddPropertyField<float, FloatField>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, float?>> propertyExpression)
		=> AddPropertyField<float, FloatField>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, double>> propertyExpression)
		=> AddPropertyField<double, DoubleField>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, double?>> propertyExpression)
		=> AddPropertyField<double, DoubleField>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal>> propertyExpression)
		=> AddPropertyField<decimal, DecimalField>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal?>> propertyExpression)
		=> AddPropertyField<decimal, DecimalField>(propertyExpression);

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumField<TEnum>>(propertyExpression);

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumField<TEnum>>(propertyExpression);

	#endregion

	#region Sub-Structure Properties

	public DataStructureBuilder<T> Structure<TStructure>(Expression<Func<T, TStructure?>> propertyExpression)
	where TStructure : class, IDataStructure, new()
		=> Structure(propertyExpression, () => new TStructure());

	public DataStructureBuilder<T> Structure<TStructure>(Expression<Func<T, TStructure?>> propertyExpression, Func<TStructure> structureFactory)
	where TStructure : class, IDataStructure
		=> AddField(new DataStructureRawPropertyField<T, TStructure>(structureFactory, propertyExpression));

	public DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>?>> propertyExpression)
	where TStructure : class, IBinaryField, new()
		=> Array(propertyExpression, () => new TStructure());

	public DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>?>> propertyExpression, Func<TStructure> entryFactory)
	where TStructure : class, IBinaryField
		=> AddField(new DataStructureCollectionField<T, TStructure>(entryFactory, propertyExpression));

	public DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, Dictionary<TKey, TValue>?>> propertyExpression)
	where TKey : class, IBinaryField, new()
	where TValue : class, IBinaryField, new()
		=> Dictionary(propertyExpression, () => new TKey(), () => new TValue());

	public DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, Dictionary<TKey, TValue>?>> propertyExpression, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryField
	where TValue : class, IBinaryField
		=> AddField(new DataStructureDictionaryField<T, TKey, TValue>(keyFactory, valueFactory, propertyExpression));

	#endregion

	#region Raw Fields

	public DataStructureBuilder<T> AddRawPropertyField<TField>(Expression<Func<T, TField?>> propertyExpression)
	where TField : class, IBinaryField, new()
		=> AddRawPropertyField(propertyExpression, () => new TField());

	public DataStructureBuilder<T> AddRawPropertyField<TField>(Expression<Func<T, TField?>> propertyExpression, Func<TField> dataTypeFactory)
	where TField : class, IBinaryField
		=> AddField(new DataStructureRawPropertyField<T, TField>(dataTypeFactory, propertyExpression));

	#endregion

	#region Property Fields

	public DataStructureBuilder<T> AddPropertyField<TProperty, TField>(Expression<Func<T, TProperty?>> propertyExpression)
	where TProperty : notnull
	where TField : IBinaryField<TProperty>, new()
		=> AddPropertyField(propertyExpression, () => new TField());

	public DataStructureBuilder<T> AddPropertyField<TProperty, TField>(Expression<Func<T, TProperty?>> propertyExpression)
	where TProperty : struct
	where TField : IBinaryField<TProperty>, new()
		=> AddPropertyField(propertyExpression, () => new TField(), NullableFieldValueAdapter<TField, TProperty>.Instance);

	public DataStructureBuilder<T> AddPropertyField<TProperty, TField>(Expression<Func<T, TProperty?>> propertyExpression, IFieldAdapter<TField, TProperty> fieldAdapter)
	where TField : IBinaryField, new()
		=> AddPropertyField(propertyExpression, () => new TField(), fieldAdapter);

	public DataStructureBuilder<T> AddPropertyField<TProperty, TField>(Expression<Func<T, TProperty?>> propertyExpression, Func<TField> dataTypeFactory)
	where TProperty : notnull
	where TField : IBinaryField<TProperty>
		=> AddPropertyField(propertyExpression, dataTypeFactory, FieldValueAdapter<TField, TProperty>.Instance);

	public DataStructureBuilder<T> AddPropertyField<TProperty, TField>(Expression<Func<T, TProperty?>> propertyExpression, Func<TField> dataTypeFactory, IFieldAdapter<TField, TProperty> fieldAdapter)
	where TField : IBinaryField
		=> AddField(new DataStructurePropertyField<T, TProperty, TField>(dataTypeFactory, propertyExpression, fieldAdapter));

	#endregion

	#region Property Bag Fields

	public DataStructureBuilder<T> PropertyBag<TPropertyKey>(
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		Func<TPropertyKey> emptyPropertyKeyFactory,
		Action<PropertyBagFieldBuilder<T>> configure,
		IEqualityComparer<TPropertyKey> keyEqualityComparer)
	where TPropertyKey : IBinaryField
		=> AddField(DataStructurePropertyBagField.Create(propertyKeyGenerator, emptyPropertyKeyFactory, configure, keyEqualityComparer));

	public DataStructureBuilder<T> PropertyBag<TPropertyKey>(
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		Func<TPropertyKey> emptyPropertyKeyFactory,
		Action<PropertyBagFieldBuilder<T>> configure)
	where TPropertyKey : IBinaryField
		=> AddField(DataStructurePropertyBagField.Create(propertyKeyGenerator, emptyPropertyKeyFactory, configure));

	public DataStructureBuilder<T> PropertyBag<TPropertyKey>(
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		Action<PropertyBagFieldBuilder<T>> configure,
		IEqualityComparer<TPropertyKey> keyEqualityComparer)
	where TPropertyKey : IBinaryField, new()
		=> AddField(DataStructurePropertyBagField.Create(propertyKeyGenerator, configure, keyEqualityComparer));

	public DataStructureBuilder<T> PropertyBag<TPropertyKey>(
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		Action<PropertyBagFieldBuilder<T>> configure)
	where TPropertyKey : IBinaryField, new()
		=> AddField(DataStructurePropertyBagField.Create(propertyKeyGenerator, configure));

	#endregion

	#region Virtual Fields

	public DataStructureBuilder<T> AddVirtualField<TField>(string? description = null)
	where TField : class, IBinaryField, new()
		=> AddField(new DataStructureVirtualField<T, TField>(() => new TField(), description));

	public DataStructureBuilder<T> AddVirtualField<TField>(TField initialValue, string? description = null)
	where TField : class, IBinaryField, new()
		=> AddField(new DataStructureVirtualField<T, TField>(() => new TField(), initialValue, description));

	public DataStructureBuilder<T> AddVirtualField<TField>(Func<TField> dataTypeFactory, string? description = null)
	where TField : class, IBinaryField
		=> AddField(new DataStructureVirtualField<T, TField>(dataTypeFactory, description));

	public DataStructureBuilder<T> AddVirtualField<TField>(Func<TField> dataTypeFactory, TField initialValue, string? description = null)
	where TField : class, IBinaryField
		=> AddField(new DataStructureVirtualField<T, TField>(dataTypeFactory, initialValue, description));

	#endregion

	public DataStructureBuilder<T> AddField(IDataStructureField<T> field)
	{
		Fields.Add(field);
		return this;
	}
}