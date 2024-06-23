using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fluent;

[PublicAPI]
public abstract class PropertyBagFieldBuilder<TStructure>
where TStructure : IDataStructure
{
	private protected PropertyBagFieldBuilder() { }

	protected Dictionary<string, IDataStructureField<TStructure>> Fields { get; } = [];

	#region String Properties

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, string?>> propertyExpression)
		=> AddPropertyField<string, TerminatedStringField>(propertyKey, propertyExpression);

	#endregion

	#region Numeric Properties

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, bool>> propertyExpression)
		=> AddPropertyField<bool, BooleanField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, bool?>> propertyExpression)
		=> AddPropertyField<bool, BooleanField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, short>> propertyExpression)
		=> AddPropertyField<short, Int16Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, short?>> propertyExpression)
		=> AddPropertyField<short, Int16Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ushort>> propertyExpression)
		=> AddPropertyField<ushort, UInt16Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ushort?>> propertyExpression)
		=> AddPropertyField<ushort, UInt16Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, int>> propertyExpression)
		=> AddPropertyField<int, Int32Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, int?>> propertyExpression)
		=> AddPropertyField<int, Int32Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, uint>> propertyExpression)
		=> AddPropertyField<uint, UInt32Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, uint?>> propertyExpression)
		=> AddPropertyField<uint, UInt32Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, long>> propertyExpression)
		=> AddPropertyField<long, Int64Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, long?>> propertyExpression)
		=> AddPropertyField<long, Int64Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ulong>> propertyExpression)
		=> AddPropertyField<ulong, UInt64Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ulong?>> propertyExpression)
		=> AddPropertyField<ulong, UInt64Field>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, float>> propertyExpression)
		=> AddPropertyField<float, FloatField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, float?>> propertyExpression)
		=> AddPropertyField<float, FloatField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, double>> propertyExpression)
		=> AddPropertyField<double, DoubleField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, double?>> propertyExpression)
		=> AddPropertyField<double, DoubleField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, decimal>> propertyExpression)
		=> AddPropertyField<decimal, DecimalField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, decimal?>> propertyExpression)
		=> AddPropertyField<decimal, DecimalField>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property<TEnum>(string propertyKey, Expression<Func<TStructure, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumField<TEnum>>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property<TEnum>(string propertyKey, Expression<Func<TStructure, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumField<TEnum>>(propertyKey, propertyExpression);

	#endregion

	#region Sub-Structure Properties

	public PropertyBagFieldBuilder<TStructure> Structure<TSubStructure>(string propertyKey, Expression<Func<TStructure, TSubStructure?>> propertyExpression)
	where TSubStructure : class, IDataStructure, new()
		=> Structure(propertyKey, propertyExpression, () => new TSubStructure());

	public PropertyBagFieldBuilder<TStructure> Structure<TSubStructure>(string propertyKey, Expression<Func<TStructure, TSubStructure?>> propertyExpression, Func<TSubStructure> structureFactory)
	where TSubStructure : class, IDataStructure
		=> AddField(propertyKey, new DataStructureRawPropertyField<TStructure, TSubStructure>(structureFactory, propertyExpression));

	public PropertyBagFieldBuilder<TStructure> Array<TItem>(string propertyKey, Expression<Func<TStructure, List<TItem>?>> propertyExpression)
	where TItem : class, IBinaryField, new()
		=> Array(propertyKey, propertyExpression, () => new TItem());

	public PropertyBagFieldBuilder<TStructure> Array<TItem>(string propertyKey, Expression<Func<TStructure, List<TItem>?>> propertyExpression, Func<TItem> entryFactory)
	where TItem : class, IBinaryField
		=> AddField(propertyKey, new DataStructureCollectionField<TStructure, TItem>(entryFactory, propertyExpression));

	public PropertyBagFieldBuilder<TStructure> Dictionary<TKey, TValue>(string propertyKey, Expression<Func<TStructure, Dictionary<TKey, TValue>?>> propertyExpression)
	where TKey : class, IBinaryField, new()
	where TValue : class, IBinaryField, new()
		=> Dictionary(propertyKey, propertyExpression, () => new TKey(), () => new TValue());

	public PropertyBagFieldBuilder<TStructure> Dictionary<TKey, TValue>(string propertyKey, Expression<Func<TStructure, Dictionary<TKey, TValue>?>> propertyExpression, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryField
	where TValue : class, IBinaryField
		=> AddField(propertyKey, new DataStructureDictionaryField<TStructure, TKey, TValue>(keyFactory, valueFactory, propertyExpression));

	#endregion

	#region Raw Fields

	public PropertyBagFieldBuilder<TStructure> AddRawPropertyField<TField>(string propertyKey, Expression<Func<TStructure, TField?>> propertyExpression)
	where TField : class, IBinaryField, new()
		=> AddRawPropertyField(propertyKey, propertyExpression, () => new TField());

	public PropertyBagFieldBuilder<TStructure> AddRawPropertyField<TField>(string propertyKey, Expression<Func<TStructure, TField?>> propertyExpression, Func<TField> dataFactory)
	where TField : class, IBinaryField
		=> AddField(propertyKey, new DataStructureRawPropertyField<TStructure, TField>(dataFactory, propertyExpression));

	#endregion

	#region Property Fields

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TField>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression)
	where TProperty : notnull
	where TField : IBinaryField<TProperty>, new()
		=> AddPropertyField(propertyKey, propertyExpression, () => new TField());

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TField>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression)
	where TProperty : struct
	where TField : IBinaryField<TProperty>, new()
		=> AddPropertyField(propertyKey, propertyExpression, () => new TField(), NullableFieldValueAdapter<TField, TProperty>.Instance);

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TField>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression, IFieldAdapter<TField, TProperty> fieldAdapter)
	where TField : IBinaryField, new()
		=> AddPropertyField(propertyKey, propertyExpression, () => new TField(), fieldAdapter);

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TValue>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression, Func<TValue> valueFactory)
	where TProperty : notnull
	where TValue : IBinaryField<TProperty>
		=> AddPropertyField(propertyKey, propertyExpression, valueFactory, FieldValueAdapter<TValue, TProperty>.Instance);

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TField>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression, Func<TField> dataFactory, IFieldAdapter<TField, TProperty> fieldAdapter)
	where TField : IBinaryField
		=> AddField(propertyKey, new DataStructurePropertyField<TStructure, TProperty, TField>(dataFactory, propertyExpression, fieldAdapter));

	#endregion

	public PropertyBagFieldBuilder<TStructure> AddField(string propertyKey, IDataStructureField<TStructure> field)
	{
		Fields.Add(propertyKey, field);
		return this;
	}
}