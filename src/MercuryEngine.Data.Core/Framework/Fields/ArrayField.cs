using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class ArrayField<TItem>(Func<TItem> itemFactory, List<TItem> initialValue) : BaseBinaryField<List<TItem>>(initialValue)
where TItem : IBinaryField
{
	private readonly Func<TItem> itemFactory = itemFactory;

	public ArrayField()
		: this(ReflectionUtility.CreateFactoryFromDefaultConstructor<TItem>()) { }

	public ArrayField(Func<TItem> itemFactory)
		: this(itemFactory, []) { }

	[JsonIgnore]
	public override uint Size => sizeof(int) + (uint) Value.Sum(e => e.Size);

	protected virtual string MappingDescription => $"Array<{typeof(TItem).Name}>";

	public override void Reset() => Value.Clear();

	public override void Read(BinaryReader reader)
	{
		Value.Clear();

		var entryCount = reader.ReadUInt32();

		for (var i = 0; i < entryCount; i++)
		{
			var entry = this.itemFactory();

			Value.Add(entry);

			try
			{
				entry.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TItem).Name}", ex);
			}
		}
	}

	public override void Write(BinaryWriter writer)
	{
		DataMapper.PushRange(MappingDescription, writer);

		writer.Write((uint) Value.Count);

		foreach (var (i, entry) in Value.Pairs())
		{
			try
			{
				DataMapper.PushRange(GetEntryMappingDescription(i, entry), writer);
				entry.WriteWithDataMapper(writer, DataMapper);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TItem).Name}", ex);
			}
			finally
			{
				DataMapper.PopRange(writer);
			}
		}

		DataMapper.PopRange(writer);
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Value.Clear();

		var entryCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < entryCount; i++)
		{
			var entry = this.itemFactory();

			Value.Add(entry);

			try
			{
				await entry.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TItem).Name}", ex);
			}
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await DataMapper.PushRangeAsync(MappingDescription, writer, cancellationToken).ConfigureAwait(false);

		await writer.WriteAsync((uint) Value.Count, cancellationToken).ConfigureAwait(false);

		foreach (var (i, entry) in Value.Pairs())
		{
			try
			{
				await DataMapper.PushRangeAsync(GetEntryMappingDescription(i, entry), writer, cancellationToken).ConfigureAwait(false);
				await entry.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TItem).Name}", ex);
			}
			finally
			{
				await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
			}
		}

		await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	protected virtual string GetEntryMappingDescription(int index, TItem entry) => $"[{index}]";
}

[PublicAPI]
public static class BinaryArray
{
	public static ArrayField<TEntry> Create<TEntry>()
	where TEntry : IBinaryField, new()
		=> new(() => new TEntry());
}