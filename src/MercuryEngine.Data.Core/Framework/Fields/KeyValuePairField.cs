using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

public class KeyValuePairField<TKey, TValue>(TKey key, TValue value) : IResettableField, IDataMapperAware
where TKey : IBinaryField
where TValue : IBinaryField
{
	public TKey   Key   { get; } = key;
	public TValue Value { get; } = value;

	public uint GetSize(uint startPosition)
	{
		var keySize = Key.GetSize(startPosition);
		var valueSize = Value.GetSize(startPosition + keySize);

		return keySize + valueSize;
	}

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

	public void Read(BinaryReader reader, ReadContext context)
	{
		DidReadKey = false;
		Key.Read(reader, context);
		DidReadKey = true;
		Value.Read(reader, context);
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		Key.WriteWithDataMapper(writer, DataMapper, context);
		Value.WriteWithDataMapper(writer, DataMapper, context);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		DidReadKey = false;
		await Key.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		DidReadKey = true;
		await Value.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await Key.WriteWithDataMapperAsync(writer, DataMapper, context, cancellationToken).ConfigureAwait(false);
		await Value.WriteWithDataMapperAsync(writer, DataMapper, context, cancellationToken).ConfigureAwait(false);
	}

	public void Deconstruct(out TKey key, out TValue value)
	{
		key = Key;
		value = Value;
	}
}