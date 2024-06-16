using MercuryEngine.Data.Core.Framework.DataTypes;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

public class DynamicStructureCollectionField<TCollection>(DynamicStructure structure, string fieldName, Func<TCollection> entryFactory) : IDynamicStructureField
where TCollection : IBinaryDataType
{
	private bool hasDataFlag;

	public DynamicStructure           Structure { get; } = structure;
	public string                     FieldName { get; } = fieldName;
	public ArrayDataType<TCollection> Data      { get; } = new(entryFactory);

	public List<TCollection> Collection          => Data.Value;
	public uint              Size                => Data.Size;
	public string            FriendlyDescription => $"<dynamic {FieldName}[array of {typeof(TCollection).Name}]>";

	public bool HasValue => this.hasDataFlag || Collection.Any();

	dynamic IDynamicStructureField.Value
	{
		get => Collection;
		set => throw new InvalidOperationException("Dynamic collection fields cannot be set");
	}

	public IDynamicStructureField Clone(DynamicStructure targetStructure)
		=> new DynamicStructureCollectionField<TCollection>(targetStructure, FieldName, entryFactory);

	public void ClearValue()
	{
		Collection.Clear();
		this.hasDataFlag = false;
	}

	public void Read(BinaryReader reader)
	{
		Data.Read(reader);
		this.hasDataFlag = true;
	}

	public void Write(BinaryWriter writer)
		=> Data.Write(writer);

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await Data.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		this.hasDataFlag = true;
	}

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> Data.WriteAsync(writer, cancellationToken);
}