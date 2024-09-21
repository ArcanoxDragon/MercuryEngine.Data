using System.Text;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Mapping;

public static class DataMappingExtensions
{
	public static void PushRange(this DataMapper? dataMapper, string description, BinaryWriter writer)
		=> dataMapper?.PushRange(description, (ulong) writer.BaseStream.GetRealPosition());

	public static async Task PushRangeAsync(this DataMapper? dataMapper, string description, AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		if (dataMapper is null)
			return;

		Stream baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

		dataMapper.PushRange(description, (ulong) baseStream.GetRealPosition());
	}

	public static void PopRange(this DataMapper? dataMapper, BinaryWriter writer)
		=> dataMapper?.PopRange((ulong) writer.BaseStream.GetRealPosition());

	public static async Task PopRangeAsync(this DataMapper? dataMapper, AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		if (dataMapper is null)
			return;

		Stream baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

		dataMapper.PopRange((ulong) baseStream.GetRealPosition());
	}

	public static void WriteWithDataMapper(this IBinaryField field, BinaryWriter writer, DataMapper? dataMapper)
	{
		if (field is IDataMapperAware dataMapperAware)
			dataMapperAware.DataMapper = dataMapper;

		field.Write(writer);
	}

	public static Task WriteWithDataMapperAsync(this IBinaryField field, AsyncBinaryWriter writer, DataMapper? dataMapper, CancellationToken cancellationToken)
	{
		if (field is IDataMapperAware dataMapperAware)
			dataMapperAware.DataMapper = dataMapper;

		return field.WriteAsync(writer, cancellationToken);
	}

	public static void WriteWithDataMapper(this DataStructureField field, BinaryWriter writer, DataMapper? dataMapper)
	{
		if (field.Handler.Field is IDataMapperAware dataMapperAwareField)
			dataMapperAwareField.DataMapper = dataMapper;
		if (field.Handler is IDataMapperAware dataMapperAwareHandler)
			dataMapperAwareHandler.DataMapper = dataMapper;

		field.Write(writer);
	}

	public static Task WriteWithDataMapperAsync(this DataStructureField field, AsyncBinaryWriter writer, DataMapper? dataMapper, CancellationToken cancellationToken)
	{
		if (field.Handler.Field is IDataMapperAware dataMapperAwareField)
			dataMapperAwareField.DataMapper = dataMapper;
		if (field.Handler is IDataMapperAware dataMapperAwareHandler)
			dataMapperAwareHandler.DataMapper = dataMapper;

		return field.WriteAsync(writer, cancellationToken);
	}

	public static string FormatRangePath(this IEnumerable<DataRange> rangePath)
	{
		var builder = new StringBuilder();
		var first = true;

		foreach (DataRange range in rangePath)
		{
			if (!first)
				builder.Append(" -> ");

			builder.Append(range.Description);
			builder.Append($" [{range.Start} : {range.End}]");
			first = false;
		}

		return builder.ToString();
	}
}