using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;
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
		=> AddPropertyField<string, TerminatedStringDataType>(propertyKey, propertyExpression);

	#endregion

	#region Numeric Properties

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, bool>> propertyExpression)
		=> AddPropertyField<bool, BoolDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, bool?>> propertyExpression)
		=> AddPropertyField<bool, BoolDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, short>> propertyExpression)
		=> AddPropertyField<short, Int16DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, short?>> propertyExpression)
		=> AddPropertyField<short, Int16DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ushort>> propertyExpression)
		=> AddPropertyField<ushort, UInt16DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ushort?>> propertyExpression)
		=> AddPropertyField<ushort, UInt16DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, int>> propertyExpression)
		=> AddPropertyField<int, Int32DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, int?>> propertyExpression)
		=> AddPropertyField<int, Int32DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, uint>> propertyExpression)
		=> AddPropertyField<uint, UInt32DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, uint?>> propertyExpression)
		=> AddPropertyField<uint, UInt32DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, long>> propertyExpression)
		=> AddPropertyField<long, Int64DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, long?>> propertyExpression)
		=> AddPropertyField<long, Int64DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ulong>> propertyExpression)
		=> AddPropertyField<ulong, UInt64DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, ulong?>> propertyExpression)
		=> AddPropertyField<ulong, UInt64DataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, float>> propertyExpression)
		=> AddPropertyField<float, FloatDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, float?>> propertyExpression)
		=> AddPropertyField<float, FloatDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, double>> propertyExpression)
		=> AddPropertyField<double, DoubleDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, double?>> propertyExpression)
		=> AddPropertyField<double, DoubleDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, decimal>> propertyExpression)
		=> AddPropertyField<decimal, DecimalDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property(string propertyKey, Expression<Func<TStructure, decimal?>> propertyExpression)
		=> AddPropertyField<decimal, DecimalDataType>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property<TEnum>(string propertyKey, Expression<Func<TStructure, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumDataType<TEnum>>(propertyKey, propertyExpression);

	public PropertyBagFieldBuilder<TStructure> Property<TEnum>(string propertyKey, Expression<Func<TStructure, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumDataType<TEnum>>(propertyKey, propertyExpression);

	#endregion

	#region Sub-Structure Properties

	public PropertyBagFieldBuilder<TStructure> Structure<TSubStructure>(string propertyKey, Expression<Func<TStructure, TSubStructure?>> propertyExpression)
	where TSubStructure : class, IDataStructure, new()
		=> Structure(propertyKey, propertyExpression, () => new TSubStructure());

	public PropertyBagFieldBuilder<TStructure> Structure<TSubStructure>(string propertyKey, Expression<Func<TStructure, TSubStructure?>> propertyExpression, Func<TSubStructure> structureFactory)
	where TSubStructure : class, IDataStructure
		=> AddField(propertyKey, new DataStructureRawPropertyField<TStructure, TSubStructure>(structureFactory, propertyExpression));

	public PropertyBagFieldBuilder<TStructure> Array<TItem>(string propertyKey, Expression<Func<TStructure, List<TItem>?>> propertyExpression)
	where TItem : class, IBinaryDataType, new()
		=> Array(propertyKey, propertyExpression, () => new TItem());

	public PropertyBagFieldBuilder<TStructure> Array<TItem>(string propertyKey, Expression<Func<TStructure, List<TItem>?>> propertyExpression, Func<TItem> entryFactory)
	where TItem : class, IBinaryDataType
		=> AddField(propertyKey, new DataStructureCollectionField<TStructure, TItem>(entryFactory, propertyExpression));

	public PropertyBagFieldBuilder<TStructure> Dictionary<TKey, TValue>(string propertyKey, Expression<Func<TStructure, Dictionary<TKey, TValue>?>> propertyExpression)
	where TKey : class, IBinaryDataType, new()
	where TValue : class, IBinaryDataType, new()
		=> Dictionary(propertyKey, propertyExpression, () => new TKey(), () => new TValue());

	public PropertyBagFieldBuilder<TStructure> Dictionary<TKey, TValue>(string propertyKey, Expression<Func<TStructure, Dictionary<TKey, TValue>?>> propertyExpression, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryDataType
	where TValue : class, IBinaryDataType
		=> AddField(propertyKey, new DataStructureDictionaryField<TStructure, TKey, TValue>(keyFactory, valueFactory, propertyExpression));

	#endregion

	#region Raw Fields

	public PropertyBagFieldBuilder<TStructure> AddRawPropertyField<TData>(string propertyKey, Expression<Func<TStructure, TData?>> propertyExpression)
	where TData : class, IBinaryDataType, new()
		=> AddRawPropertyField(propertyKey, propertyExpression, () => new TData());

	public PropertyBagFieldBuilder<TStructure> AddRawPropertyField<TData>(string propertyKey, Expression<Func<TStructure, TData?>> propertyExpression, Func<TData> dataTypeFactory)
	where TData : class, IBinaryDataType
		=> AddField(propertyKey, new DataStructureRawPropertyField<TStructure, TData>(dataTypeFactory, propertyExpression));

	#endregion

	#region Property Fields

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TData>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression)
	where TProperty : notnull
	where TData : IBinaryDataType<TProperty>, new()
		=> AddPropertyField(propertyKey, propertyExpression, () => new TData());

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TData>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression)
	where TProperty : struct
	where TData : IBinaryDataType<TProperty>, new()
		=> AddPropertyField(propertyKey, propertyExpression, () => new TData(), NullableDataTypeWithValueAdapter<TData, TProperty>.Instance);

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TData>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression, IDataAdapter<TData, TProperty> dataAdapter)
	where TData : IBinaryDataType, new()
		=> AddPropertyField(propertyKey, propertyExpression, () => new TData(), dataAdapter);

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TData>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression, Func<TData> dataTypeFactory)
	where TProperty : notnull
	where TData : IBinaryDataType<TProperty>
		=> AddPropertyField(propertyKey, propertyExpression, dataTypeFactory, DataTypeWithValueAdapter<TData, TProperty>.Instance);

	public PropertyBagFieldBuilder<TStructure> AddPropertyField<TProperty, TData>(string propertyKey, Expression<Func<TStructure, TProperty?>> propertyExpression, Func<TData> dataTypeFactory, IDataAdapter<TData, TProperty> dataAdapter)
	where TData : IBinaryDataType
		=> AddField(propertyKey, new DataStructurePropertyField<TStructure, TProperty, TData>(dataTypeFactory, propertyExpression, dataAdapter));

	#endregion

	public PropertyBagFieldBuilder<TStructure> AddField(string propertyKey, IDataStructureField<TStructure> field)
	{
		Fields.Add(propertyKey, field);
		return this;
	}
}