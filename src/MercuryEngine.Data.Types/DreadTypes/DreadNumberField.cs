using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Types.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.DreadTypes;

public class DreadNumberField<T, TBaseField>(string typeName) : IBinaryField<T>, ITypedDreadField, IDataMapperAware
where T : unmanaged
where TBaseField : NumberField<T>, new()
{
	private readonly TBaseField baseField = new();

	public string TypeName { get; } = typeName;

	public T Value
	{
		get => this.baseField.Value;
		set => this.baseField.Value = value;
	}

	public DataMapper? DataMapper
	{
		get => ( (IDataMapperAware) this.baseField ).DataMapper;
		set => ( (IDataMapperAware) this.baseField ).DataMapper = value;
	}

	public uint Size => this.baseField.Size;

	public void Read(BinaryReader reader, ReadContext context)
		=> this.baseField.Read(reader, context);

	public void Write(BinaryWriter writer, WriteContext context)
		=> this.baseField.Write(writer, context);

	public Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
		=> this.baseField.ReadAsync(reader, context, cancellationToken);

	public Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
		=> this.baseField.WriteAsync(writer, context, cancellationToken: cancellationToken);

	public void Reset()
		=> this.baseField.Reset();
}