namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

public class DynamicStructureCollectionField<TCollection> : IDynamicStructureField
where TCollection : IBinaryDataType
{
	private readonly Func<TCollection> entryFactory;

	public DynamicStructureCollectionField(DynamicStructure structure, string fieldName, Func<TCollection> entryFactory)
	{
		this.entryFactory = entryFactory;
		Structure = structure;
		FieldName = fieldName;
		Data = new ArrayDataType<TCollection>(entryFactory);
	}

	public DynamicStructure           Structure { get; }
	public string                     FieldName { get; }
	public ArrayDataType<TCollection> Data      { get; }

	public List<TCollection> Collection          => Data.Value;
	public uint              Size                => Data.Size;
	public string            FriendlyDescription => $"<dynamic {FieldName}[array of {typeof(TCollection).Name}]>";

	public bool HasValue => Collection.Any();

	dynamic IDynamicStructureField.Value
	{
		get => Collection;
		set => throw new InvalidOperationException("Dynamic collection fields cannot be set");
	}

	public IDynamicStructureField Clone(DynamicStructure targetStructure)
		=> new DynamicStructureCollectionField<TCollection>(targetStructure, FieldName, this.entryFactory);

	public void ClearValue()
		=> Collection.Clear();

	public void Read(BinaryReader reader)
		=> Data.Read(reader);

	public void Write(BinaryWriter writer)
		=> Data.Write(writer);
}