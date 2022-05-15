using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fluent;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DynamicStructure"/>.
/// </summary>
[PublicAPI]
public abstract class DynamicStructureBuilder
{
	private protected DynamicStructureBuilder() { }

	protected abstract DynamicStructure StructureBeingBuilt { get; }

	#region String Fields

	public DynamicStructureBuilder String(string fieldName)
		=> AddField<string, TerminatedStringDataType>(fieldName);

	#endregion

	#region Numeric Fields

	public DynamicStructureBuilder Bool(string fieldName)
		=> AddField<bool, BoolDataType>(fieldName);

	public DynamicStructureBuilder Int16(string fieldName)
		=> AddField<short, Int16DataType>(fieldName);

	public DynamicStructureBuilder UInt16(string fieldName)
		=> AddField<ushort, UInt16DataType>(fieldName);

	public DynamicStructureBuilder Int32(string fieldName)
		=> AddField<int, Int32DataType>(fieldName);

	public DynamicStructureBuilder UInt32(string fieldName)
		=> AddField<uint, UInt32DataType>(fieldName);

	public DynamicStructureBuilder Int64(string fieldName)
		=> AddField<long, Int64DataType>(fieldName);

	public DynamicStructureBuilder UInt64(string fieldName)
		=> AddField<ulong, UInt64DataType>(fieldName);

	public DynamicStructureBuilder Float(string fieldName)
		=> AddField<float, FloatDataType>(fieldName);

	public DynamicStructureBuilder Double(string fieldName)
		=> AddField<double, DoubleDataType>(fieldName);

	public DynamicStructureBuilder Decimal(string fieldName)
		=> AddField<decimal, DecimalDataType>(fieldName);

	public DynamicStructureBuilder Enum<TEnum>(string fieldName)
	where TEnum : struct, Enum
		=> AddField<TEnum, EnumDataType<TEnum>>(fieldName);

	#endregion

	#region Sub-Structures

	public DynamicStructureBuilder Structure<TStructure>(string fieldName, Func<TStructure> factory)
	where TStructure : class, IDataStructure
		=> AddFieldInternal(new DynamicStructureRawField<TStructure>(StructureBeingBuilt, fieldName, factory));

	public DynamicStructureBuilder Structure<TStructure>(string fieldName)
	where TStructure : class, IDataStructure, new()
		=> AddFieldInternal(new DynamicStructureRawField<TStructure>(StructureBeingBuilt, fieldName, () => new TStructure()));

	public DynamicStructureBuilder Array<TCollection>(string fieldName)
	where TCollection : class, IBinaryDataType, new()
		=> AddFieldInternal(new DynamicStructureCollectionField<TCollection>(StructureBeingBuilt, fieldName, () => new TCollection()));

	public DynamicStructureBuilder Array<TCollection>(string fieldName, Func<TCollection> entryFactory)
	where TCollection : class, IBinaryDataType
		=> AddFieldInternal(new DynamicStructureCollectionField<TCollection>(StructureBeingBuilt, fieldName, entryFactory));

	#endregion

	#region Raw Fields

	public DynamicStructureBuilder AddField<TData>(string fieldName, Func<TData> dataTypeFactory)
	where TData : class, IBinaryDataType
		=> AddFieldInternal(new DynamicStructureRawField<TData>(StructureBeingBuilt, fieldName, dataTypeFactory));

	public DynamicStructureBuilder AddField<TValue, TData>(string fieldName)
	where TValue : notnull
	where TData : class, IBinaryDataType<TValue>, new()
		=> AddField(fieldName, new DataTypeWithValueAdapter<TData, TValue>());

	public DynamicStructureBuilder AddField<TValue, TData>(string fieldName, IDataAdapter<TData, TValue> dataAdapter)
	where TValue : notnull
	where TData : IBinaryDataType, new()
		=> AddFieldInternal(new DynamicStructureField<TValue, TData>(StructureBeingBuilt, fieldName, () => new TData(), dataAdapter));

	public DynamicStructureBuilder AddField<TValue, TData>(string fieldName, Func<TData> dataTypeFactory, IDataAdapter<TData, TValue> dataAdapter)
	where TValue : notnull
	where TData : IBinaryDataType
		=> AddFieldInternal(new DynamicStructureField<TValue, TData>(StructureBeingBuilt, fieldName, dataTypeFactory, dataAdapter));

	#endregion

	/// <summary>
	/// Adds a copy of the provided <paramref name="otherField"/> to the structure being built.
	/// </summary>
	public DynamicStructureBuilder AddCopy(IDynamicStructureField otherField)
		=> AddFieldInternal(otherField.Clone(StructureBeingBuilt));

	protected abstract void AddField(IDynamicStructureField field);

	private DynamicStructureBuilder AddFieldInternal(IDynamicStructureField field)
	{
		AddField(field);
		return this;
	}
}