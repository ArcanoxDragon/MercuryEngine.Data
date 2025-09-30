using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types;

[PublicAPI]
public static class LinkedListField
{
	public static LinkedListField<T> Create<T>(uint startByteAlignment = 0, uint endByteAlignment = 0)
	where T : IBinaryField, new()
		=> new(() => new T(), startByteAlignment, endByteAlignment);
}

public class LinkedListField<T>(Func<T> entryFactory, uint startByteAlignment = 0, uint endByteAlignment = 0) : IResettableField, IDataMapperAware
where T : IBinaryField
{
	public List<T?> Entries { get; } = [];

	public uint GetSize(uint startPosition) => 2 * sizeof(ulong) * (uint) Entries.Count;

	public DataMapper? DataMapper { get; set; }

	public void Reset()
	{
		Entries.Clear();
	}

	public void Read(BinaryReader reader, ReadContext context)
	{
		ulong targetAddress;
		ulong nextNodeAddress;

		if (!reader.BaseStream.CanSeek)
			throw new NotSupportedException($"Cannot read {typeof(LinkedListField<T>).GetDisplayName()} field because the underlying stream does not support seeking");

		Entries.Clear();

		var i = 0;
		var prevPosition = reader.BaseStream.Position;

		do
		{
			targetAddress = reader.ReadUInt64();
			nextNodeAddress = reader.ReadUInt64();

			if (targetAddress > 0)
			{
				if (context.HeapManager.TryGetField(targetAddress, out var field) && field is T entry)
				{
					Entries.Add(entry);
				}
				else
				{
					SeekSafe(reader.BaseStream, targetAddress, $"Target address of element {i}");

					// Read the target
					var target = entryFactory();

					target.Read(reader, context);
					context.HeapManager.Register(targetAddress, target, endByteAlignment);
					Entries.Add(target);
				}
			}
			else
			{
				Entries.Add(default);
			}

			if (nextNodeAddress > 0)
				SeekSafe(reader.BaseStream, nextNodeAddress, $"Next node address of element {i}");

			i++;
		}
		while (nextNodeAddress > 0);

		reader.BaseStream.Position = prevPosition;
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		// While nodes *could* be stored in any random order in the stream, it's substantially easier to write them sequentially, so that's what we do
		for (var i = 0; i < Entries.Count; i++)
		{
			var entry = Entries[i];
			var targetAddress = entry is null ? 0 : context.HeapManager.GetAddressOrAllocate(entry, startByteAlignment, endByteAlignment);

			DataMapper.PushRange($"{typeof(LinkedListField<T>).GetDisplayName()}[{i}]: pointer to target @ 0x{targetAddress:X16}", writer);
			writer.Write(targetAddress);
			DataMapper.PopRange(writer);

			if (i == Entries.Count - 1)
			{
				// Write "0" for the end of the list
				DataMapper.PushRange($"{typeof(LinkedListField<T>).GetDisplayName()}[{i}]: end of linked list", writer);
				writer.Write(0ul);
			}
			else
			{
				// Write the address where we will start the next node
				var nextNodeAddress = (ulong) ( writer.BaseStream.Position + sizeof(ulong) );

				DataMapper.PushRange($"{typeof(LinkedListField<T>).GetDisplayName()}[{i}]: pointer to next node @ 0x{nextNodeAddress:X16}", writer);
				writer.Write(nextNodeAddress);
			}

			DataMapper.PopRange(writer);
		}
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		ulong targetAddress;
		ulong nextNodeAddress;

		if (!reader.BaseStream.CanSeek)
			throw new NotSupportedException($"Cannot read {typeof(LinkedListField<T>).GetDisplayName()} field because the underlying stream does not support seeking");

		Entries.Clear();

		var i = 0;
		var prevPosition = reader.BaseStream.Position;

		do
		{
			targetAddress = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);
			nextNodeAddress = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

			if (targetAddress > 0)
			{
				if (context.HeapManager.TryGetField(targetAddress, out var field) && field is T entry)
				{
					Entries.Add(entry);
				}
				else
				{
					SeekSafe(reader.BaseStream, targetAddress, $"Target address of element {i}");

					// Read the target
					var target = entryFactory();

					await target.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
					context.HeapManager.Register(targetAddress, target, endByteAlignment);
					Entries.Add(target);
				}
			}

			if (nextNodeAddress > 0)
				SeekSafe(reader.BaseStream, targetAddress, $"Next node address of element {i}");

			i++;
		}
		while (nextNodeAddress > 0);

		reader.BaseStream.Position = prevPosition;
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		// While nodes *could* be stored in any random order in the stream, it's substantially easier to write them sequentially, so that's what we do
		for (var i = 0; i < Entries.Count; i++)
		{
			var entry = Entries[i];
			var targetAddress = entry is null ? 0 : context.HeapManager.GetAddressOrAllocate(entry, startByteAlignment, endByteAlignment);

			await DataMapper.PushRangeAsync($"{typeof(LinkedListField<T>).GetDisplayName()}[{i}]: pointer to target @ 0x{targetAddress:X16}", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(targetAddress, cancellationToken).ConfigureAwait(false);
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);

			if (i == Entries.Count - 1)
			{
				// Write "0" for the end of the list
				await DataMapper.PushRangeAsync($"{typeof(LinkedListField<T>).GetDisplayName()}[{i}]: end of linked list", writer, cancellationToken).ConfigureAwait(false);
				await writer.WriteAsync(0ul, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				// Write the address where we will start the next node
				var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
				var nextNodeAddress = (ulong) ( baseStream.Position + sizeof(ulong) );

				await DataMapper.PushRangeAsync($"{typeof(LinkedListField<T>).GetDisplayName()}[{i}]: pointer to next node @ 0x{nextNodeAddress:X16}", writer, cancellationToken).ConfigureAwait(false);
				await writer.WriteAsync(nextNodeAddress, cancellationToken).ConfigureAwait(false);
			}

			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}

	private void SeekSafe(Stream stream, ulong address, FormattableString addressType)
	{
		if (address >= (ulong) stream.Length)
			throw new IOException($"{addressType} of {typeof(LinkedListField<T>).GetDisplayName()} field was beyond the end of the stream");

		stream.Seek((long) address, SeekOrigin.Begin);
	}
}