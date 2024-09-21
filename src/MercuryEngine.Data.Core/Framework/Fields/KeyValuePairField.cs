using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

public class KeyValuePairField<TKey, TValue>(TKey key, TValue value) : IResettableField, IDataMapperAware
where TKey : IBinaryField
where TValue : IBinaryField
{
	public TKey   Key   { get; } = key;
	public TValue Value { get; } = value;

	[JsonIgnore]
	public uint Size => Key.Size + Value.Size;

	[JsonIgnore]
	public bool HasMeaningfulData => Value.HasMeaningfulData;

	protected DataMapper? DataMapper { get; set; }

	internal bool DidReadKey { get; private set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public void Reset()
	{
		( Key as IResettableField )?.Reset();
		( Value as IResettableField )?.Reset();
		DidReadKey = false;
	}

	public void Read(BinaryReader reader)
	{
		DidReadKey = false;
		Key.Read(reader);
		DidReadKey = true;
		Value.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		Key.WriteWithDataMapper(writer, DataMapper);
		Value.WriteWithDataMapper(writer, DataMapper);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		DidReadKey = false;
		await Key.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
		DidReadKey = true;
		await Value.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await Key.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
		await Value.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
	}

	public void Deconstruct(out TKey key, out TValue value)
	{
		key = Key;
		value = Value;
	}
}