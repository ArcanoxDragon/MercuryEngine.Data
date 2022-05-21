using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

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
	private readonly Func<TData>                 dataTypeFactory;
	private readonly IDataAdapter<TData, TValue> dataAdapter;

	public DynamicStructureField(DynamicStructure structure, string fieldName, Func<TData> dataTypeFactory, IDataAdapter<TData, TValue> dataAdapter)
	{
		Structure = structure;
		FieldName = fieldName;
		Data = dataTypeFactory();

		this.dataTypeFactory = dataTypeFactory;
		this.dataAdapter = dataAdapter;
	}

	public DynamicStructure Structure { get; }
	public string           FieldName { get; }
	public TData            Data      { get; private set; }

	public uint   Size                => Data.Size;
	public string FriendlyDescription => $"<dynamic {FieldName}[{typeof(TValue).Name}, {typeof(TData).Name}]>";

	public bool HasValue { get; private set; }

	public TValue Value
	{
		get => this.dataAdapter.Get(Data);
		set
		{
			var data = Data;

			this.dataAdapter.Put(ref data, value);

			if (!ReferenceEquals(data, Data))
				Data = data;
		}
	}

	dynamic IDynamicStructureField.Value
	{
		get => Value;
		set
		{
			HasValue = true;
			Value = value;
		}
	}

	public IDynamicStructureField Clone(DynamicStructure targetStructure)
		=> new DynamicStructureField<TValue, TData>(targetStructure, FieldName, this.dataTypeFactory, this.dataAdapter);

	public void ClearValue()
		=> HasValue = false;

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