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
		InnerData = initialValue;
	}

	public ITypedDreadField? InnerData { get; set; }

	private DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public ulong InnerTypeId => InnerData?.TypeId ?? 0L;
	public uint  Size        => InnerData?.Size ?? 0;

	public void Read(BinaryReader reader)
	{
		var typeId = reader.ReadUInt64();

		if (typeId is NullTypeId)
		{
			// Read NULL; clear out inner value and return
			InnerData = null;
			return;
		}

		InnerData = DreadTypeRegistry.GetTypedField(typeId);
		InnerData.Read(reader);
	}

	public void Write(BinaryWriter writer)
	{
		try
		{
			if (InnerData is null)
			{
				DataMapper.PushRange("type-prefixed: NULL", writer);

				// Write an all-zeroes type ID to indicate NULL
				writer.Write(NullTypeId);
				return;
			}

			DataMapper.PushRange($"type-prefixed: {InnerData.TypeName}", writer);
			writer.Write(InnerData.TypeId);
			InnerData.WriteWithDataMapper(writer, DataMapper);
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
			InnerData = null;
			return;
		}

		InnerData = DreadTypeRegistry.GetTypedField(typeId);
		await InnerData.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		try
		{
			if (InnerData is null)
			{
				await DataMapper.PushRangeAsync("type-prefixed: NULL", writer, cancellationToken).ConfigureAwait(false);

				// Write an all-zeroes type ID to indicate NULL
				await writer.WriteAsync(NullTypeId, cancellationToken).ConfigureAwait(false);
				return;
			}

			await DataMapper.PushRangeAsync($"type-prefixed: {InnerData.TypeName}", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(InnerData.TypeId, cancellationToken).ConfigureAwait(false);
			await InnerData.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}
}