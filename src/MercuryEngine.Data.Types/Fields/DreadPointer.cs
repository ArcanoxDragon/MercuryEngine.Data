using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Fields;

[PublicAPI]
public class DreadPointer<TField> : IBinaryField, IDataMapperAware
where TField : class, ITypedDreadField
{
	private const ulong NullTypeId = 0UL;

	public DreadPointer() { }

	public DreadPointer(TField initialValue)
	{
		Value = initialValue;
	}

	public TField? Value { get; set; }

	private DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public ulong InnerTypeId => Value?.TypeId ?? 0L;

	[JsonIgnore]
	public uint Size => Value?.Size ?? 0;

	[JsonIgnore]
	public bool HasMeaningfulData => Value is { HasMeaningfulData: true };

	public void Read(BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			Value = null;
			return;
		}

		var candidate = DreadTypeLibrary.GetTypedField(typeId);

		if (candidate is not TField field)
			throw new IOException($"Expected to read a {typeof(TField).FullName}, but the data indicated a type of {candidate.GetType().FullName}");

		Value = field;
		Value.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		try
		{
			if (Value is null)
			{
				DataMapper.PushRange("type-prefixed: NULL", writer);

				// Write an all-zeroes type ID to indicate NULL
				writer.Write(NullTypeId);
				return;
			}

			DataMapper.PushRange($"type-prefixed: {Value.TypeName}", writer);
			writer.Write(Value.TypeId);
			Value.WriteWithDataMapper(writer, DataMapper);
		}
		finally
		{
			DataMapper.PopRange(writer);
		}
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		var typeId = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			Value = null;
			return;
		}

		var candidate = DreadTypeLibrary.GetTypedField(typeId);

		if (candidate is not TField field)
			throw new IOException($"Expected to read a {typeof(TField).FullName}, but the data indicated a type of {candidate.GetType().FullName}");

		Value = field;
		await Value.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		try
		{
			if (Value is null)
			{
				await DataMapper.PushRangeAsync("type-prefixed: NULL", writer, cancellationToken).ConfigureAwait(false);

				// Write an all-zeroes type ID to indicate NULL
				await writer.WriteAsync(NullTypeId, cancellationToken).ConfigureAwait(false);
				return;
			}

			await DataMapper.PushRangeAsync($"type-prefixed: {Value.TypeName}", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(Value.TypeId, cancellationToken).ConfigureAwait(false);
			await Value.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}
}