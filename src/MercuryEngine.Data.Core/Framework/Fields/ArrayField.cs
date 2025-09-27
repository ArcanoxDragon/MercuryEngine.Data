using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

// TODO: Rename to ListField or something
[PublicAPI]
public class ArrayField<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TItem
>(Func<TItem> itemFactory, List<TItem> initialValue, uint startByteAlignment, byte paddingByte = 0) : BaseBinaryField<List<TItem>>(initialValue)
where TItem : IBinaryField
{
	protected static readonly Func<TItem> DefaultItemFactory = ReflectionUtility.CreateFactoryFromDefaultConstructor<TItem>();

	private readonly Func<TItem> itemFactory = itemFactory;

	private byte[] paddingBuffer = [];

	public ArrayField()
		: this(DefaultItemFactory) { }

	public ArrayField(Func<TItem> itemFactory)
		: this(itemFactory, []) { }

	public ArrayField(Func<TItem> itemFactory, List<TItem> initialValue)
		: this(itemFactory, [], startByteAlignment: 0) { }

	public ArrayField(uint startByteAlignment, byte paddingByte = 0)
		: this(DefaultItemFactory, startByteAlignment, paddingByte) { }

	public ArrayField(Func<TItem> itemFactory, uint startByteAlignment, byte paddingByte = 0)
		: this(itemFactory, [], startByteAlignment, paddingByte) { }

	public override uint GetSize(uint startPosition)
	{
		var totalSize = (uint) sizeof(uint); // Count
		var currentPosition = startPosition + totalSize;

		if (Value.Count == 0)
			return totalSize;

		if (startByteAlignment > 0)
		{
			var neededPadding = MathHelper.GetNeededPaddingForAlignment(currentPosition, startByteAlignment);

			totalSize += neededPadding;
			currentPosition += neededPadding;
		}

		foreach (var entry in Value)
		{
			var entrySize = entry.GetSize(currentPosition);

			totalSize += entrySize;
			currentPosition += entrySize;
		}

		return totalSize;
	}

	protected virtual string MappingDescription => $"Array<{typeof(TItem).Name}>";

	public override void Reset() => Value.Clear();

	public override void Read(BinaryReader reader, ReadContext context)
	{
		Value.Clear();

		var entryCount = reader.ReadUInt32();

		if (entryCount == 0)
			return;

		if (startByteAlignment > 0)
		{
			var paddingReadBytes = reader.BaseStream.GetNeededPaddingForAlignment(startByteAlignment);

			reader.ReadBytes((int) paddingReadBytes);
		}

		for (var i = 0; i < entryCount; i++)
		{
			var startPosition = reader.BaseStream.GetRealPosition();
			var entry = this.itemFactory();

			Value.Add(entry);

			try
			{
				entry.Read(reader, context);
			}
			catch (Exception ex)
			{
				throw new IOException(GetEntryReadExceptionMessage(i, entry, startPosition), ex);
			}
		}
	}

	public override void Write(BinaryWriter writer, WriteContext context)
	{
		DataMapper.PushRange(MappingDescription, writer);

		try
		{
			writer.Write((uint) Value.Count);

			if (Value.Count == 0)
				return;

			if (startByteAlignment > 0)
			{
				var neededPadding = (int) writer.BaseStream.GetNeededPaddingForAlignment(startByteAlignment);

				EnsurePaddingBuffer(neededPadding);
				writer.Write(this.paddingBuffer[..neededPadding]);
			}

			foreach (var (i, entry) in Value.Pairs())
			{
				try
				{
					DataMapper.PushRange(GetEntryMappingDescription(i, entry), writer);
					entry.WriteWithDataMapper(writer, DataMapper, context);
				}
				catch (Exception ex)
				{
					throw new IOException(GetEntryWriteExceptionMessage(i, entry), ex);
				}
				finally
				{
					DataMapper.PopRange(writer);
				}
			}
		}
		finally
		{
			DataMapper.PopRange(writer);
		}
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		Value.Clear();

		var entryCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		if (entryCount == 0)
			return;

		if (startByteAlignment > 0)
		{
			var paddingReadBytes = reader.BaseStream.GetNeededPaddingForAlignment(startByteAlignment);

			await reader.ReadBytesAsync((int) paddingReadBytes, cancellationToken).ConfigureAwait(false);
		}

		for (var i = 0; i < entryCount; i++)
		{
			var startPosition = reader.BaseStream.GetRealPosition();
			var entry = this.itemFactory();

			Value.Add(entry);

			try
			{
				await entry.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new IOException(GetEntryReadExceptionMessage(i, entry, startPosition), ex);
			}
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await DataMapper.PushRangeAsync(MappingDescription, writer, cancellationToken).ConfigureAwait(false);

		try
		{
			await writer.WriteAsync((uint) Value.Count, cancellationToken).ConfigureAwait(false);

			if (Value.Count == 0)
				return;

			if (startByteAlignment > 0)
			{
				var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
				var neededPadding = (int) baseStream.GetNeededPaddingForAlignment(startByteAlignment);

				EnsurePaddingBuffer(neededPadding);
				await writer.WriteAsync(this.paddingBuffer[..neededPadding], cancellationToken).ConfigureAwait(false);
			}

			foreach (var (i, entry) in Value.Pairs())
			{
				try
				{
					await DataMapper.PushRangeAsync(GetEntryMappingDescription(i, entry), writer, cancellationToken).ConfigureAwait(false);
					await entry.WriteWithDataMapperAsync(writer, DataMapper, context, cancellationToken).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new IOException(GetEntryWriteExceptionMessage(i, entry), ex);
				}
				finally
				{
					await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
				}
			}
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}

	protected virtual string GetEntryReadExceptionMessage(int index, TItem entry, long position)
		=> $"An exception occurred while reading entry {index} of an array of {typeof(TItem).Name} (position: {position})";

	protected virtual string GetEntryWriteExceptionMessage(int index, TItem entry)
		=> $"An exception occurred while writing entry {index} of an array of {typeof(TItem).Name}";

	protected virtual string GetEntryMappingDescription(int index, TItem entry) => $"[{index}]";

	private void EnsurePaddingBuffer(int neededPadding)
	{
		if (this.paddingBuffer.Length >= neededPadding)
			return;

		var newBuffer = new byte[BitOperations.RoundUpToPowerOf2((uint) neededPadding)];

		Array.Fill(newBuffer, paddingByte);
		this.paddingBuffer = newBuffer;
	}
}

[PublicAPI]
public static class ArrayField
{
	public static ArrayField<TEntry> Create<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TEntry
	>(uint startByteAlignment, byte paddingByte = 0)
	where TEntry : IBinaryField, new()
		=> new(() => new TEntry(), startByteAlignment, paddingByte);

	public static ArrayField<TEntry> Create<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TEntry
	>()
	where TEntry : IBinaryField, new()
		=> new(() => new TEntry());
}