using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataAdapters;
using MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

namespace MercuryEngine.Data.Framework.DataTypes.Structures;

[PublicAPI]
public abstract class DataStructure<T> : IDataStructure, IBinaryDataType<T>
where T : DataStructure<T>
{
	private readonly List<IDataStructureField> fields = new();

	protected DataStructure()
	{
		var builder = new Builder((T) this);

		// ReSharper disable once VirtualMemberCallInConstructor
		Describe(builder);
	}

	protected abstract void Describe(DataStructureBuilder<T> builder);

	public uint Size => (uint) this.fields.Sum(f => f.Size);

	public void Read(BinaryReader reader)
	{
		foreach (var (i, field) in this.fields.Pairs())
		{
			try
			{
				field.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	public void Write(BinaryWriter writer)
	{
		foreach (var (i, field) in this.fields.Pairs())
		{
			try
			{
				field.Write(writer);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	#region IBinaryDataType<T> Explicit Implementation

	T IBinaryDataType<T>.Value
	{
		get => (T) this;
		set => throw new InvalidOperationException($"DataStructure types consumed as {nameof(IBinaryDataType<T>)} cannot be assigned a value.");
	}

	#endregion

	#region Builder

	private sealed class Builder : DataStructureBuilder<T>
	{
		private readonly T owner;

		public Builder(T owner)
		{
			this.owner = owner;
		}

		#region String Literals

		public override DataStructureBuilder<T> Literal(string text)
			=> AddVirtualField(new TerminatedStringDataType(text));

		public override DataStructureBuilder<T> Literal(string text, string description)
			=> AddVirtualField(new TerminatedStringDataType(text), description);

		#endregion

		#region Numeric Literals

		public override DataStructureBuilder<T> Int16(short value)
			=> AddVirtualField(new Int16DataType { Value = value });

		public override DataStructureBuilder<T> Int16(short value, string description)
			=> AddVirtualField(new Int16DataType { Value = value }, description);

		public override DataStructureBuilder<T> UInt16(ushort value)
			=> AddVirtualField(new UInt16DataType { Value = value });

		public override DataStructureBuilder<T> UInt16(ushort value, string description)
			=> AddVirtualField(new UInt16DataType { Value = value }, description);

		public override DataStructureBuilder<T> Int32(int value)
			=> AddVirtualField(new Int32DataType { Value = value });

		public override DataStructureBuilder<T> Int32(int value, string description)
			=> AddVirtualField(new Int32DataType { Value = value }, description);

		public override DataStructureBuilder<T> UInt32(uint value)
			=> AddVirtualField(new UInt32DataType { Value = value });

		public override DataStructureBuilder<T> UInt32(uint value, string description)
			=> AddVirtualField(new UInt32DataType { Value = value }, description);

		public override DataStructureBuilder<T> Int64(long value)
			=> AddVirtualField(new Int64DataType { Value = value });

		public override DataStructureBuilder<T> Int64(long value, string description)
			=> AddVirtualField(new Int64DataType { Value = value }, description);

		public override DataStructureBuilder<T> UInt64(ulong value)
			=> AddVirtualField(new UInt64DataType { Value = value });

		public override DataStructureBuilder<T> UInt64(ulong value, string description)
			=> AddVirtualField(new UInt64DataType { Value = value }, description);

		public override DataStructureBuilder<T> Float(float value)
			=> AddVirtualField(new FloatDataType { Value = value });

		public override DataStructureBuilder<T> Float(float value, string description)
			=> AddVirtualField(new FloatDataType { Value = value }, description);

		public override DataStructureBuilder<T> Double(double value)
			=> AddVirtualField(new DoubleDataType { Value = value });

		public override DataStructureBuilder<T> Double(double value, string description)
			=> AddVirtualField(new DoubleDataType { Value = value }, description);

		public override DataStructureBuilder<T> Decimal(decimal value)
			=> AddVirtualField(new DecimalDataType { Value = value });

		public override DataStructureBuilder<T> Decimal(decimal value, string description)
			=> AddVirtualField(new DecimalDataType { Value = value }, description);

		public override DataStructureBuilder<T> Enum<TEnum>(TEnum value)
			=> AddVirtualField(new EnumDataType<TEnum> { Value = value });

		public override DataStructureBuilder<T> Enum<TEnum>(TEnum value, string description)
			=> AddVirtualField(new EnumDataType<TEnum> { Value = value }, description);

		#endregion

		#region String Properties

		public override DataStructureBuilder<T> String(Expression<Func<T, string>> propertyExpression)
			=> AddPropertyField<string, TerminatedStringDataType>(propertyExpression);

		#endregion

		#region Numeric Properties

		public override DataStructureBuilder<T> Int16(Expression<Func<T, short>> propertyExpression)
			=> AddPropertyField<short, Int16DataType>(propertyExpression);

		public override DataStructureBuilder<T> UInt16(Expression<Func<T, ushort>> propertyExpression)
			=> AddPropertyField<ushort, UInt16DataType>(propertyExpression);

		public override DataStructureBuilder<T> Int32(Expression<Func<T, int>> propertyExpression)
			=> AddPropertyField<int, Int32DataType>(propertyExpression);

		public override DataStructureBuilder<T> UInt32(Expression<Func<T, uint>> propertyExpression)
			=> AddPropertyField<uint, UInt32DataType>(propertyExpression);

		public override DataStructureBuilder<T> Int64(Expression<Func<T, long>> propertyExpression)
			=> AddPropertyField<long, Int64DataType>(propertyExpression);

		public override DataStructureBuilder<T> UInt64(Expression<Func<T, ulong>> propertyExpression)
			=> AddPropertyField<ulong, UInt64DataType>(propertyExpression);

		public override DataStructureBuilder<T> Float(Expression<Func<T, float>> propertyExpression)
			=> AddPropertyField<float, FloatDataType>(propertyExpression);

		public override DataStructureBuilder<T> Double(Expression<Func<T, double>> propertyExpression)
			=> AddPropertyField<double, DoubleDataType>(propertyExpression);

		public override DataStructureBuilder<T> Decimal(Expression<Func<T, decimal>> propertyExpression)
			=> AddPropertyField<decimal, DecimalDataType>(propertyExpression);

		public override DataStructureBuilder<T> Enum<TEnum>(Expression<Func<T, TEnum>> propertyExpression)
			=> AddPropertyField<TEnum, EnumDataType<TEnum>>(propertyExpression);

		#endregion

		#region Sub-Structures

		public override DataStructureBuilder<T> Structure<TStructure>(Expression<Func<T, TStructure>> propertyExpression)
			=> AddField(new DataStructureRawPropertyField<T, TStructure>(this.owner, propertyExpression));

		public override DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>>> propertyExpression)
			=> Array(propertyExpression, () => new TStructure());

		public override DataStructureBuilder<T> Array<TStructure>(Expression<Func<T, List<TStructure>>> propertyExpression, Func<TStructure> entryFactory)
			=> AddField(new DataStructureCollectionField<T, TStructure>(this.owner, propertyExpression, entryFactory));

		public override DataStructureBuilder<T> Dictionary<TKey, TValue>(Expression<Func<T, Dictionary<TKey, TValue>>> propertyExpression)
			=> AddField(new DataStructureDictionaryField<T, TKey, TValue>(this.owner, propertyExpression));

		#endregion

		#region Raw Fields

		public DataStructureBuilder<T> AddField(IDataStructureField field)
		{
			this.owner.fields.Add(field);
			return this;
		}

		public override DataStructureBuilder<T> AddRawPropertyField<TData>(Expression<Func<T, TData>> propertyExpression)
			=> AddField(new DataStructureRawPropertyField<T, TData>(this.owner, propertyExpression));

		public override DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty>> propertyExpression)
			=> AddPropertyField(propertyExpression, new BinaryDataTypeWithValueAdapter<TProperty, TData>());

		public override DataStructureBuilder<T> AddPropertyField<TProperty, TData>(Expression<Func<T, TProperty>> propertyExpression, IDataAdapter<TProperty, TData> dataAdapter)
			=> AddField(new DataStructurePropertyField<T, TProperty, TData>(this.owner, propertyExpression, dataAdapter));

		public override DataStructureBuilder<T> AddVirtualField<TData>(TData data)
			=> AddField(new DataStructureVirtualField<T, TData>(this.owner, data));

		public override DataStructureBuilder<T> AddVirtualField<TData>(TData data, string description)
			=> AddField(new DataStructureVirtualField<T, TData>(this.owner, data, description));

		#endregion
	}

	#endregion
}