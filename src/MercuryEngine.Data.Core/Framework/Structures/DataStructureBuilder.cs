using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="T">The type of structure this <see cref="DataStructureBuilder{T}"/> can build.</typeparam>
[PublicAPI]
public abstract class DataStructureBuilder<T>
where T : IDataStructure
{
	private protected DataStructureBuilder() { }

	protected List<IDataStructureField<T>> Fields { get; } = new();

	#region String Literals

	public DataStructureBuilder<T> Literal(string text, string? description = null)
		=> AddVirtualField(new TerminatedStringDataType(text), description);

	#endregion

	#region Numeric Literals

	public DataStructureBuilder<T> Literal(short value, string? description = null)
		=> AddVirtualField(new Int16DataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(ushort value, string? description = null)
		=> AddVirtualField(new UInt16DataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(int value, string? description = null)
		=> AddVirtualField(new Int32DataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(uint value, string? description = null)
		=> AddVirtualField(new UInt32DataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(long value, string? description = null)
		=> AddVirtualField(new Int64DataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(ulong value, string? description = null)
		=> AddVirtualField(new UInt64DataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(float value, string? description = null)
		=> AddVirtualField(new FloatDataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(double value, string? description = null)
		=> AddVirtualField(new DoubleDataType { Value = value }, description);

	public DataStructureBuilder<T> Literal(decimal value, string? description = null)
		=> AddVirtualField(new DecimalDataType { Value = value }, description);

	public DataStructureBuilder<T> Literal<TEnum>(TEnum value, string? description = null)
	where TEnum : struct, Enum
		=> AddVirtualField(new EnumDataType<TEnum> { Value = value }, description);

	#endregion

	#region String Properties

	public DataStructureBuilder<T> Property(Expression<Func<T, string?>> propertyExpression)
		=> AddPropertyField<string, TerminatedStringDataType>(propertyExpression);

	#endregion

	#region Numeric Properties

	public DataStructureBuilder<T> Property(Expression<Func<T, short>> propertyExpression)
		=> AddPropertyField<short, Int16DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, short?>> propertyExpression)
		=> AddPropertyField<short, Int16DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort>> propertyExpression)
		=> AddPropertyField<ushort, UInt16DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort?>> propertyExpression)
		=> AddPropertyField<ushort, UInt16DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, int>> propertyExpression)
		=> AddPropertyField<int, Int32DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, int?>> propertyExpression)
		=> AddPropertyField<int, Int32DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, uint>> propertyExpression)
		=> AddPropertyField<uint, UInt32DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, uint?>> propertyExpression)
		=> AddPropertyField<uint, UInt32DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, long>> propertyExpression)
		=> AddPropertyField<long, Int64DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, long?>> propertyExpression)
		=> AddPropertyField<long, Int64DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong>> propertyExpression)
		=> AddPropertyField<ulong, UInt64DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong?>> propertyExpression)
		=> AddPropertyField<ulong, UInt64DataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, float>> propertyExpression)
		=> AddPropertyField<float, FloatDataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, float?>> propertyExpression)
		=> AddPropertyField<float, FloatDataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, double>> propertyExpression)
		=> AddPropertyField<double, DoubleDataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, double?>> propertyExpression)
		=> AddPropertyField<double, DoubleDataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal>> propertyExpression)
		=> AddPropertyField<decimal, DecimalDataType>(propertyExpression);

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal?>> propertyExpression)
		=> AddPropertyField<decimal, DecimalDataType>(propertyExpression);

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumDataType<TEnum>>(propertyExpression);

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> AddPropertyField<TEnum, EnumDataType<TEnum>>(propertyExpression);

	#endregion

	#region Sub-Structure Properties

	public DataStructureBuilder<T> Structure<TStructure>(Expression<Func<T, TStructure?>> propertyExpression)
	where TStructure : class, IDataStructure, new()
		=> Structure(propertyExpression, () => new TStructure());

	public DataStructureBuilder<T> Structure<TStructure>(Expression<Func<T, TStructure?>> propertyExpression, Func<TStructure> structureFactory)
	where TStructure : class, IDataStructure
		=> AddField(new DataStructureRawPropertyField<T, TStructure>(structureFactory, propertyExpression));

	public DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>?>> propertyExpression)
	where TStructure : class, IBinaryDataType, new()
		=> Array(propertyExpression, () => new TStructure());

	public DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>?>> propertyExpression, Func<TStructure> entryFactory)
	where TStructure : class, IBinaryDataType
		=> AddField(new DataStructureCollectionField<T, TStructure>(entryFactory, propertyExpression));

	public DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, Dictionary<TKey, TValue>?>> propertyExpression)
	where TKey : class, IBinaryDataType, new()
	where TValue : class, IBinaryDataType, new()
		=> Dictionary(propertyExpression, () => new TKey(), () => new TValue());

	public DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, Dictionary<TKey, TValue>?>> propertyExpression, Func<TKey> keyFactory, Func<TValue> valueFactory)
	where TKey : class, IBinaryDataType
	where TValue : class, IBinaryDataType
		=> AddField(new DataStructureDictionaryField<T, TKey, TValue>(keyFactory, valueFactory, propertyExpression));

	#endregion

	#region Raw Fields

	public DataStructureBuilder<T> AddRawPropertyField<TData>(Expression<Func<T, TData?>> propertyExpression)
	where TData : class, IBinaryDataType, new()
		=> AddRawPropertyField(propertyExpression, () => new TData());

	public DataStructureBuilder<T> AddRawPropertyField<TData>(Expression<Func<T, TData?>> propertyExpression, Func<TData> dataTypeFactory)
	where TData : class, IBinaryDataType
		=> AddField(new DataStructureRawPropertyField<T, TData>(dataTypeFactory, propertyExpression));

	#endregion

	#region Property Fields

	public DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty?>> propertyExpression)
	where TProperty : notnull
	where TData : IBinaryDataType<TProperty>, new()
		=> AddPropertyField(propertyExpression, () => new TData());

	public DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty?>> propertyExpression)
	where TProperty : struct
	where TData : IBinaryDataType<TProperty>, new()
		=> AddPropertyField(propertyExpression, () => new TData(), NullableDataTypeWithValueAdapter<TData, TProperty>.Instance);

	public DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty?>> propertyExpression, IDataAdapter<TData, TProperty> dataAdapter)
	where TData : IBinaryDataType, new()
		=> AddPropertyField(propertyExpression, () => new TData(), dataAdapter);

	public DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty?>> propertyExpression, Func<TData> dataTypeFactory)
	where TProperty : notnull
	where TData : IBinaryDataType<TProperty>
		=> AddPropertyField(propertyExpression, dataTypeFactory, DataTypeWithValueAdapter<TData, TProperty>.Instance);

	public DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty?>> propertyExpression, Func<TData> dataTypeFactory, IDataAdapter<TData, TProperty> dataAdapter)
	where TData : IBinaryDataType
		=> AddField(new DataStructurePropertyField<T, TProperty, TData>(dataTypeFactory, propertyExpression, dataAdapter));

	#endregion

	#region Virtual Fields

	public DataStructureBuilder<T> AddVirtualField<TData>(string? description = null)
	where TData : class, IBinaryDataType, new()
		=> AddField(new DataStructureVirtualField<T, TData>(() => new TData(), description));

	public DataStructureBuilder<T> AddVirtualField<TData>(TData initialValue, string? description = null)
	where TData : class, IBinaryDataType, new()
		=> AddField(new DataStructureVirtualField<T, TData>(() => new TData(), initialValue, description));

	public DataStructureBuilder<T> AddVirtualField<TData>(Func<TData> dataTypeFactory, string? description = null)
	where TData : class, IBinaryDataType
		=> AddField(new DataStructureVirtualField<T, TData>(dataTypeFactory, description));

	public DataStructureBuilder<T> AddVirtualField<TData>(Func<TData> dataTypeFactory, TData initialValue, string? description = null)
	where TData : class, IBinaryDataType
		=> AddField(new DataStructureVirtualField<T, TData>(dataTypeFactory, initialValue, description));

	#endregion

	public DataStructureBuilder<T> AddField(IDataStructureField<T> field)
	{
		Fields.Add(field);
		return this;
	}
}