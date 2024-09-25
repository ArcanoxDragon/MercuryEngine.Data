using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class DictionaryField<TKey, TValue>(Func<TKey> keyFactory, Func<TValue> valueFactory)
	: BaseBinaryField<OrderedMultiDictionary<TKey, TValue>>(new OrderedMultiDictionary<TKey, TValue>())
where TKey : IBinaryField
where TValue : IBinaryField
{
	private readonly Func<TKey>   keyFactory   = keyFactory;
	private readonly Func<TValue> valueFactory = valueFactory;

	public DictionaryField() : this(
		ReflectionUtility.CreateFactoryFromDefaultConstructor<TKey>(),
		ReflectionUtility.CreateFactoryFromDefaultConstructor<TValue>()
	) { }

	[JsonIgnore]
	public override uint Size => sizeof(uint) + (uint) Value.Sum(pair => pair.Key.Size + pair.Value.Size);

	protected virtual string MappingDescription => $"Dictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

	public override void Reset() => Value.Clear();

	public override void Read(BinaryReader reader)
	{
		Value.Clear();

		var entryCount = reader.ReadUInt32();

		for (var i = 0; i < entryCount; i++)
		{
			var entry = new KeyValuePairField<TKey, TValue>(this.keyFactory(), this.valueFactory());

			try
			{
				entry.Read(reader);
				Value.Add(entry.Key, entry.Value);
			}
			catch (Exception ex)
			{
				throw new IOException(GetEntryReadExceptionMessage(i, entry), ex);
			}
		}
	}

	public override void Write(BinaryWriter writer)
	{
		DataMapper.PushRange(MappingDescription, writer);

		writer.Write((uint) Value.Count);

		var i = 0;

		foreach (var (key, value) in Value)
		{
			var entry = new KeyValuePairField<TKey, TValue>(key, value);

			try
			{
				DataMapper.PushRange(GetEntryMappingDescription(i, entry), writer);
				entry.WriteWithDataMapper(writer, DataMapper);
			}
			catch (Exception ex)
			{
				throw new IOException(GetEntryWriteExceptionMessage(i, entry), ex);
			}
			finally
			{
				DataMapper.PopRange(writer);
			}

			i++;
		}

		DataMapper.PopRange(writer);
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Value.Clear();

		var entryCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < entryCount; i++)
		{
			var entry = new KeyValuePairField<TKey, TValue>(this.keyFactory(), this.valueFactory());

			try
			{
				await entry.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
				Value.Add(entry.Key, entry.Value);
			}
			catch (Exception ex)
			{
				throw new IOException(GetEntryReadExceptionMessage(i, entry), ex);
			}
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await DataMapper.PushRangeAsync(MappingDescription, writer, cancellationToken).ConfigureAwait(false);

		await writer.WriteAsync((uint) Value.Count, cancellationToken).ConfigureAwait(false);

		var i = 0;

		foreach (var (key, value) in Value)
		{
			var entry = new KeyValuePairField<TKey, TValue>(key, value);

			try
			{
				await DataMapper.PushRangeAsync(GetEntryMappingDescription(i, entry), writer, cancellationToken).ConfigureAwait(false);
				await entry.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException(GetEntryWriteExceptionMessage(i, entry), ex);
			}
			finally
			{
				await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
			}

			i++;
		}

		await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	protected virtual string GetEntryReadExceptionMessage(int index, KeyValuePairField<TKey, TValue> entry)
	{
		if (entry.DidReadKey)
			return $"An exception occurred while reading key \"{entry.Key}\" (index {index}) of a dictionary of {typeof(TKey).Name} -> {typeof(TValue).Name}";

		return $"An exception occurred while reading entry {index} of a dictionary of {typeof(TKey).Name} -> {typeof(TValue).Name}";
	}

	protected virtual string GetEntryWriteExceptionMessage(int index, KeyValuePairField<TKey, TValue> entry)
		=> $"An exception occurred while writing key \"{entry.Key}\" (index {index}) of a dictionary of {typeof(TKey).Name} -> {typeof(TValue).Name}";

	protected virtual string GetEntryMappingDescription(int index, KeyValuePairField<TKey, TValue> entry)
		=> $"key: {entry.Key}";
}

[PublicAPI]
public static class DictionaryField
{
	public static DictionaryField<TKey, TValue> Create<TKey, TValue>()
	where TKey : IBinaryField, new()
	where TValue : IBinaryField, new()
		=> new(() => new TKey(), () => new TValue());
}