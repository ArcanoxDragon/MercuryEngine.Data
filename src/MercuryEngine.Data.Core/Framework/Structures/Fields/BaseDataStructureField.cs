using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

public abstract class BaseDataStructureField<TStructure, TField>(Func<TField> fieldFactory) : IDataStructureField<TStructure>, IDataMapperAware
where TStructure : IDataStructure
where TField : IBinaryField
{
	public abstract string FriendlyDescription { get; }

	protected DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public abstract void ClearData(TStructure structure);
	public abstract bool HasData(TStructure structure);

	public virtual uint GetSize(TStructure structure) => GetFieldForStorage(structure).Size;

	public virtual void Read(TStructure structure, BinaryReader reader)
	{
		var data = CreateFieldInstance();

		data.Read(reader);
		LoadFieldFromStorage(structure, data);
	}

	public virtual void Write(TStructure structure, BinaryWriter writer)
	{
		if (!HasData(structure))
			return;

		var data = GetFieldForStorage(structure);

		data.WriteWithDataMapper(writer, DataMapper);
	}

	public async Task ReadAsync(TStructure structure, AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		var data = CreateFieldInstance();

		await data.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		LoadFieldFromStorage(structure, data);
	}

	public async Task WriteAsync(TStructure structure, AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		if (!HasData(structure))
			return;

		var data = GetFieldForStorage(structure);

		await data.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
	}

	protected abstract TField GetFieldForStorage(TStructure structure);
	protected abstract void LoadFieldFromStorage(TStructure structure, TField data);

	protected virtual TField CreateFieldInstance() => fieldFactory();
}