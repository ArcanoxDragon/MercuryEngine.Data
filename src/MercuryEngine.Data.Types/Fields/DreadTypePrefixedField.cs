using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Fields;

[PublicAPI]
public class DreadTypePrefixedField : IBinaryField, IDataMapperAware
{
	private const ulong NullTypeId = 0UL;

	public DreadTypePrefixedField() { }

	public DreadTypePrefixedField(ITypedDreadField initialValue)
	{
		TypedField = initialValue;
	}

	public IBinaryField? InnerData
	{
		get => TypedField switch {
			TypedFieldWrapper wrapper => wrapper.WrappedField,
			_                         => TypedField,
		};
		set => TypedField = value switch {
			ITypedDreadField typedField => typedField,
			null                        => null,

			_ => throw new NotSupportedException($"Only {nameof(ITypedDreadField)} instances may be assigned to {nameof(InnerData)}"),
		};
	}

	internal ITypedDreadField? TypedField { get; set; }

	private DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public ulong InnerTypeId => TypedField?.TypeId ?? 0L;
	public uint  Size        => TypedField?.Size ?? 0;

	public void Read(BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			TypedField = null;
			return;
		}

		TypedField = DreadTypeLibrary.GetTypedField(typeId);
		TypedField.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		try
		{
			if (TypedField is null)
			{
				DataMapper.PushRange("type-prefixed: NULL", writer);

				// Write an all-zeroes type ID to indicate NULL
				writer.Write(NullTypeId);
				return;
			}

			DataMapper.PushRange($"type-prefixed: {TypedField.TypeName}", writer);
			writer.Write(TypedField.TypeId);
			TypedField.WriteWithDataMapper(writer, DataMapper);
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
			TypedField = null;
			return;
		}

		TypedField = DreadTypeLibrary.GetTypedField(typeId);
		await TypedField.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		try
		{
			if (TypedField is null)
			{
				await DataMapper.PushRangeAsync("type-prefixed: NULL", writer, cancellationToken).ConfigureAwait(false);

				// Write an all-zeroes type ID to indicate NULL
				await writer.WriteAsync(NullTypeId, cancellationToken).ConfigureAwait(false);
				return;
			}

			await DataMapper.PushRangeAsync($"type-prefixed: {TypedField.TypeName}", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(TypedField.TypeId, cancellationToken).ConfigureAwait(false);
			await TypedField.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}
}