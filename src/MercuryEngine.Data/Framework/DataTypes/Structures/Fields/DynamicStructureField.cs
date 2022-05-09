using JetBrains.Annotations;
using MercuryEngine.Data.Framework.DataAdapters;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// Represents a field on a <see cref="DynamicStructure"/> that is accessed using dynamic typing as opposed to a statically defined property.
/// </summary>
/// <typeparam name="TValue">The managed type that the property will have on the <see cref="DynamicStructure"/>.</typeparam>
/// <typeparam name="TData">The data type that the value is stored as in binary data.</typeparam>
[PublicAPI]
public class DynamicStructureField<TValue, TData> : IDynamicStructureField
where TValue : notnull
where TData : IBinaryDataType
{
	private readonly IDataAdapter<TValue, TData> dataAdapter;

	public DynamicStructureField(DynamicStructure structure, string fieldName, TData initialValue, IDataAdapter<TValue, TData> dataAdapter)
	{
		Structure = structure;
		FieldName = fieldName;
		Data = initialValue;

		this.dataAdapter = dataAdapter;
	}

	public DynamicStructure Structure { get; }
	public string           FieldName { get; }
	public TData            Data      { get; }

	public uint   Size                => Data.Size;
	public string FriendlyDescription => $"<dynamic {FieldName}[{typeof(TValue).Name}, {typeof(TData).Name}]>";

	IBinaryDataType IDynamicStructureField.Data => Data;

	public TValue Value
	{
		get => this.dataAdapter.Get(Data);
		set => this.dataAdapter.Put(Data, value);
	}

	dynamic IDynamicStructureField.Value
	{
		get => Value;
		set => Value = value;
	}

	public void Read(BinaryReader reader)
		=> Data.Read(reader);

	public void Write(BinaryWriter writer)
		=> Data.Write(writer);
}