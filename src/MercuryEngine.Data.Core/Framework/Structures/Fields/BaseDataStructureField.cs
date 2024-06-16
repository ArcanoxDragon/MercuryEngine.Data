using MercuryEngine.Data.Core.Framework.DataTypes;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

public abstract class BaseDataStructureField<TStructure, TData>(Func<TData> dataTypeFactory) : IDataStructureField<TStructure>
where TStructure : IDataStructure
where TData : IBinaryDataType
{
	public abstract string FriendlyDescription { get; }

	public abstract void ClearData(TStructure structure);
	public abstract bool HasData(TStructure structure);

	public virtual uint GetSize(TStructure structure) => GetData(structure).Size;

	public virtual void Read(TStructure structure, BinaryReader reader)
	{
		var data = CreateDataType();

		data.Read(reader);
		PutData(structure, data);
	}

	public virtual void Write(TStructure structure, BinaryWriter writer)
	{
		if (!HasData(structure))
			return;

		var data = GetData(structure);

		data.Write(writer);
	}

	public async Task ReadAsync(TStructure structure, AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		var data = CreateDataType();

		await data.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		PutData(structure, data);
	}

	public async Task WriteAsync(TStructure structure, AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		if (!HasData(structure))
			return;

		var data = GetData(structure);

		await data.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	protected abstract TData GetData(TStructure structure);
	protected abstract void PutData(TStructure structure, TData data);

	protected virtual TData CreateDataType() => dataTypeFactory();
}