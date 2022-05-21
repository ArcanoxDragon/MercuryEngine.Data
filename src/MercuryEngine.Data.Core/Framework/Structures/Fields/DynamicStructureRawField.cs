using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// Represents a field on a <see cref="DynamicStructure"/> that is accessed using dynamic typing as opposed to a statically defined property.
/// </summary>
/// <typeparam name="TData">The data type that the value is stored as in binary data.</typeparam>
[PublicAPI]
public class DynamicStructureRawField<TData> : IDynamicStructureField
where TData : IBinaryDataType
{
	private readonly Func<TData> dataTypeFactory;

	public DynamicStructureRawField(DynamicStructure structure, string fieldName, Func<TData> dataTypeFactory)
	{
		this.dataTypeFactory = dataTypeFactory;

		Structure = structure;
		FieldName = fieldName;
		Data = dataTypeFactory();
	}

	public DynamicStructure Structure { get; }
	public string           FieldName { get; }
	public TData            Data      { get; private set; }

	public uint   Size                => Data.Size;
	public string FriendlyDescription => $"<dynamic {FieldName}[{typeof(TData).Name}]>";

	public bool HasValue { get; private set; }

	dynamic IDynamicStructureField.Value
	{
		get => Data;
		set
		{
			HasValue = true;
			Data = value;
		}
	}

	public IDynamicStructureField Clone(DynamicStructure targetStructure)
		=> new DynamicStructureRawField<TData>(targetStructure, FieldName, this.dataTypeFactory);

	public void ClearValue()
	{
		Data = this.dataTypeFactory();
		HasValue = false;
	}

	public void Read(BinaryReader reader)
	{
		Data.Read(reader);
		HasValue = true;
	}

	public void Write(BinaryWriter writer)
		=> Data.Write(writer);

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		await Data.ReadAsync(reader, cancellationToken);
		HasValue = true;
	}

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> Data.WriteAsync(writer, cancellationToken);
}