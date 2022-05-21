using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.DataTypes;

[PublicAPI]
public class ArrayDataType<TEntry> : BaseDataType<List<TEntry>>
where TEntry : IBinaryDataType
{
	private readonly Func<TEntry> entryFactory;

	/// <summary>
	/// Constructor that uses reflection to construct data types
	/// TODO: Find alternative way to do this
	/// </summary>
	public ArrayDataType() : this(Activator.CreateInstance<TEntry>) { }

	public ArrayDataType(Func<TEntry> entryFactory) : this(entryFactory, new List<TEntry>()) { }

	public ArrayDataType(Func<TEntry> entryFactory, List<TEntry> initialValue) : base(initialValue)
	{
		this.entryFactory = entryFactory;
	}

	public override uint Size => (uint) Value.Sum(e => e.Size);

	public override void Read(BinaryReader reader)
	{
		Value.Clear();

		var entryCount = reader.ReadUInt32();

		for (var i = 0; i < entryCount; i++)
		{
			var entry = this.entryFactory();

			Value.Add(entry);

			try
			{
				entry.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TEntry).Name}", ex);
			}
		}
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write((uint) Value.Count);

		foreach (var (i, entry) in Value.Pairs())
		{
			try
			{
				entry.Write(writer);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TEntry).Name}", ex);
			}
		}
	}

	public override async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		Value.Clear();

		var entryCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		for (var i = 0; i < entryCount; i++)
		{
			var entry = this.entryFactory();

			Value.Add(entry);

			try
			{
				await entry.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TEntry).Name}", ex);
			}
		}
	}

	public override async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await writer.WriteAsync((uint) Value.Count, cancellationToken).ConfigureAwait(false);

		foreach (var (i, entry) in Value.Pairs())
		{
			try
			{
				await entry.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading entry {i} of an array of {typeof(TEntry).Name}", ex);
			}
		}
	}
}

public static class ArrayDataType
{
	public static ArrayDataType<TEntry> Create<TEntry>()
	where TEntry : IBinaryDataType, new()
		=> new(() => new TEntry());
}