namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

public class DynamicStructureCollectionField<TCollection> : IDynamicStructureField
where TCollection : IBinaryDataType, new()
{
	public DynamicStructureCollectionField(DynamicStructure structure, string fieldName)
	{
		Structure = structure;
		FieldName = fieldName;
	}

	public DynamicStructure           Structure { get; }
	public string                     FieldName { get; }
	public ArrayDataType<TCollection> Data      { get; } = ArrayDataType.Create<TCollection>();

	public List<TCollection> Collection          => Data.Value;
	public uint              Size                => (uint) Collection.Sum(i => i.Size);
	public string            FriendlyDescription => $"<dynamic {FieldName}[array of {typeof(TCollection).Name}]>";

	IBinaryDataType IDynamicStructureField.Data => Data;

	dynamic IDynamicStructureField.Value
	{
		get => Collection;
		set => throw new InvalidOperationException("Dynamic collection fields cannot be set");
	}

	public void Read(BinaryReader reader)
		=> Data.Read(reader);

	public void Write(BinaryWriter writer)
		=> Data.Write(writer);
}