using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Framework.DataAdapters;

namespace MercuryEngine.Data.Framework.DataTypes.Structures;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="T">The type of structure this <see cref="DataStructureBuilder{T}"/> can build.</typeparam>
[PublicAPI]
public abstract class DataStructureBuilder<T>
where T : IDataStructure
{
	private protected DataStructureBuilder() { }

	#region String Literals

	public abstract DataStructureBuilder<T> Literal(string text);
	public abstract DataStructureBuilder<T> Literal(string text, string description);

	#endregion

	#region Numeric Literals

	public abstract DataStructureBuilder<T> Int16(short value);
	public abstract DataStructureBuilder<T> Int16(short value, string description);
	public abstract DataStructureBuilder<T> UInt16(ushort value);
	public abstract DataStructureBuilder<T> UInt16(ushort value, string description);
	public abstract DataStructureBuilder<T> Int32(int value);
	public abstract DataStructureBuilder<T> Int32(int value, string description);
	public abstract DataStructureBuilder<T> UInt32(uint value);
	public abstract DataStructureBuilder<T> UInt32(uint value, string description);
	public abstract DataStructureBuilder<T> Int64(long value);
	public abstract DataStructureBuilder<T> Int64(long value, string description);
	public abstract DataStructureBuilder<T> UInt64(ulong value);
	public abstract DataStructureBuilder<T> UInt64(ulong value, string description);
	public abstract DataStructureBuilder<T> Float(float value);
	public abstract DataStructureBuilder<T> Float(float value, string description);
	public abstract DataStructureBuilder<T> Double(double value);
	public abstract DataStructureBuilder<T> Double(double value, string description);
	public abstract DataStructureBuilder<T> Decimal(decimal value);
	public abstract DataStructureBuilder<T> Decimal(decimal value, string description);

	public abstract DataStructureBuilder<T> Enum<TEnum>(TEnum value)
	where TEnum : struct, Enum;

	public abstract DataStructureBuilder<T> Enum<TEnum>(TEnum value, string description)
	where TEnum : struct, Enum;

	#endregion

	#region String Properties

	public abstract DataStructureBuilder<T> String(Expression<Func<T, string>> propertyExpression);

	#endregion

	#region Numeric Properties

	public abstract DataStructureBuilder<T> Int16(Expression<Func<T, short>> propertyExpression);
	public abstract DataStructureBuilder<T> UInt16(Expression<Func<T, ushort>> propertyExpression);
	public abstract DataStructureBuilder<T> Int32(Expression<Func<T, int>> propertyExpression);
	public abstract DataStructureBuilder<T> UInt32(Expression<Func<T, uint>> propertyExpression);
	public abstract DataStructureBuilder<T> Int64(Expression<Func<T, long>> propertyExpression);
	public abstract DataStructureBuilder<T> UInt64(Expression<Func<T, ulong>> propertyExpression);
	public abstract DataStructureBuilder<T> Float(Expression<Func<T, float>> propertyExpression);
	public abstract DataStructureBuilder<T> Double(Expression<Func<T, double>> propertyExpression);
	public abstract DataStructureBuilder<T> Decimal(Expression<Func<T, decimal>> propertyExpression);

	public abstract DataStructureBuilder<T> Enum<TEnum>(Expression<Func<T, TEnum>> propertyExpression)
	where TEnum : struct, Enum;

	#endregion

	#region Sub-Structures

	public abstract DataStructureBuilder<T> Structure<TStructure>(Expression<Func<T, TStructure>> propertyExpression)
	where TStructure : class, IDataStructure;

	public abstract DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>>> propertyExpression)
	where TStructure : class, IBinaryDataType, new();

	public abstract DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>>> propertyExpression, Func<TStructure> entryFactory)
	where TStructure : class, IBinaryDataType;

	public abstract DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, Dictionary<TKey, TValue>>> propertyExpression)
	where TKey : class, IBinaryDataType, new()
	where TValue : class, IBinaryDataType, new();

	#endregion

	#region Raw Fields

	public abstract DataStructureBuilder<T> AddRawPropertyField<TData>(Expression<Func<T, TData>> propertyExpression)
	where TData : class, IBinaryDataType;

	public abstract DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty>> propertyExpression)
	where TProperty : notnull
	where TData : IBinaryDataType<TProperty>, new();

	public abstract DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty>> propertyExpression, IDataAdapter<TProperty, TData> dataAdapter)
	where TProperty : notnull
	where TData : IBinaryDataType, new();

	public abstract DataStructureBuilder<T> AddVirtualField<TData>(TData data)
	where TData : class, IBinaryDataType;

	public abstract DataStructureBuilder<T> AddVirtualField<TData>(TData data, string description)
	where TData : class, IBinaryDataType;

	#endregion
}