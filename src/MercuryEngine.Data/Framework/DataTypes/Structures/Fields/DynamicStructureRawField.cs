using JetBrains.Annotations;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// Represents a field on a <see cref="DynamicStructure"/> that is accessed using dynamic typing as opposed to a statically defined property.
/// </summary>
/// <typeparam name="TData">The data type that the value is stored as in binary data.</typeparam>
[PublicAPI]
public class DynamicStructureRawField<TData> : IDynamicStructureField
where TData : IBinaryDataType
{
	public DynamicStructureRawField(DynamicStructure structure, string fieldName, TData initialValue)
	{
		Structure = structure;
		FieldName = fieldName;
		Data = initialValue;
	}

	public DynamicStructure Structure { get; }
	public string           FieldName { get; }
	public TData            Data      { get; set; }

	public uint   Size                => Data.Size;
	public string FriendlyDescription => $"<dynamic {FieldName}[{typeof(TData).Name}]>";

	IBinaryDataType IDynamicStructureField.Data => Data;

	dynamic IDynamicStructureField.Value
	{
		get => Data;
		set => Data = value;
	}

	public void Read(BinaryReader reader)
		=> Data.Read(reader);

	public void Write(BinaryWriter writer)
		=> Data.Write(writer);
}