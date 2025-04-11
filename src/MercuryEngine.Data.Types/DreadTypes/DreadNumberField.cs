using MercuryEngine.Data.Core.Framework.Fields;
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

	public void Read(BinaryReader reader)
		=> this.baseField.Read(reader);

	public void Write(BinaryWriter writer)
		=> this.baseField.Write(writer);

	public Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		=> this.baseField.ReadAsync(reader, cancellationToken);

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		=> this.baseField.WriteAsync(writer, cancellationToken);

	public void Reset()
		=> this.baseField.Reset();
}