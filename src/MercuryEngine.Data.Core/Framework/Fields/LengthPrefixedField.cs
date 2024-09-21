using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

public class LengthPrefixedField<TField>(TField innerField, bool validateReads = true) : IBinaryField, IDataMapperAware
where TField : IBinaryField
{
	[JsonIgnore]
	public uint Size => sizeof(uint) + InnerField.Size;

	[JsonIgnore]
	public bool HasMeaningfulData => InnerField.HasMeaningfulData;

	public TField InnerField    { get; set; } = innerField;
	public bool   ValidateReads { get; set; } = validateReads;

	protected DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public void Reset() => ( InnerField as IResettableField )?.Reset();

	public void Read(BinaryReader reader)
	{
		var dataSize = reader.ReadUInt32();

		if (dataSize == 0)
		{
			if (InnerField is IResettableField resettableField)
				resettableField.Reset();

			return;
		}

		if (ValidateReads)
		{
			// Use a SlicedStream to ensure the inner field can't read beyond the designated block

			using var slicedStream = new SlicedStream(reader.BaseStream, reader.BaseStream.Position, dataSize);
			using var innerReader = new BinaryReader(slicedStream);

			InnerField.Read(innerReader);

			if (slicedStream.Position != dataSize)
				throw new IOException($"Expected {nameof(InnerField)} to read {dataSize} bytes, but only {slicedStream.Position} were read");

			slicedStream.Seek(0, SeekOrigin.End);
		}
		else
		{
			// Allow the inner field to read as much or as little as it wants, but always seek the stream
			// to exactly "dataSize" bytes beyond the start position once the inner field has finished reading.

			var startPosition = reader.BaseStream.Position;

			InnerField.Read(reader);
			reader.BaseStream.Seek(startPosition + dataSize, SeekOrigin.Begin);
		}
	}

	public void Write(BinaryWriter writer)
	{
		if (!InnerField.HasMeaningfulData)
		{
			writer.Write(0U);
			return;
		}

		var startPosition = writer.BaseStream.GetRealPosition();
		var writeOffset = (ulong) startPosition + sizeof(uint);

		using var _ = DataMapper?.PushOffset(writeOffset) ?? EmptyDisposable.Instance;

		using var memoryStream = new MemoryStream();
		using var innerWriter = new BinaryWriter(memoryStream);

		InnerField.WriteWithDataMapper(innerWriter, DataMapper);
		innerWriter.Flush();

		writer.Write((uint) memoryStream.Length);

		memoryStream.Seek(0, SeekOrigin.Begin);
		memoryStream.CopyTo(writer.BaseStream);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		var dataSize = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

		if (dataSize == 0)
		{
			if (InnerField is IResettableField resettableField)
				resettableField.Reset();

			return;
		}

		if (ValidateReads)
		{
			// Use a SlicedStream to ensure the inner field can't read beyond the designated block

			await using var slicedStream = new SlicedStream(reader.BaseStream, reader.BaseStream.Position, dataSize);
			using var innerReader = new AsyncBinaryReader(slicedStream);

			await InnerField.ReadAsync(innerReader, cancellationToken).ConfigureAwait(false);

			if (slicedStream.Position != dataSize)
				throw new IOException($"Expected {nameof(InnerField)} to read {dataSize} bytes, but only {slicedStream.Position} were read");

			slicedStream.Seek(0, SeekOrigin.End);
		}
		else
		{
			// Allow the inner field to read as much or as little as it wants, but always seek the stream
			// to exactly "dataSize" bytes beyond the start position once the inner field has finished reading.

			var startPosition = reader.BaseStream.Position;

			await InnerField.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			reader.BaseStream.Seek(startPosition + dataSize, SeekOrigin.Begin);
		}
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		if (!InnerField.HasMeaningfulData)
		{
			await writer.WriteAsync(0U, cancellationToken).ConfigureAwait(false);
			return;
		}

		var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);
		var startPosition = baseStream.GetRealPosition();
		var writeOffset = (ulong) startPosition + sizeof(uint);

		using var _ = DataMapper?.PushOffset(writeOffset) ?? EmptyDisposable.Instance;

		using var memoryStream = new MemoryStream();
		using var innerWriter = new AsyncBinaryWriter(memoryStream);

		await InnerField.WriteWithDataMapperAsync(innerWriter, DataMapper, cancellationToken).ConfigureAwait(false);
		await innerWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

		await writer.WriteAsync((uint) memoryStream.Length, cancellationToken).ConfigureAwait(false);

		memoryStream.Seek(0, SeekOrigin.Begin);
		await memoryStream.CopyToAsync(baseStream, cancellationToken).ConfigureAwait(false);
	}
}