using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public abstract class DataStructure<T> : IDataStructure, IBinaryField<T>, IDataMapperAware
where T : DataStructure<T>, IDescribeDataStructure<T>
{
	protected DataStructure()
	{
		var builder = new Builder((T) this);

		T.Describe(builder);

		Fields = builder.Build();
	}

	[JsonIgnore]
	public uint Size => (uint) Fields.Sum(f => f.Size);

	[JsonIgnore]
	public DataMapper? DataMapper { get; set; }

	protected IEnumerable<DataStructureField> Fields { get; }

	public void Reset()
	{
		foreach (var field in Fields)
			field.Reset();
	}

	#region I/O

	public void Read(BinaryReader reader)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				field.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.Description}) of {GetType().Name} (position: {reader.BaseStream.Position})", ex);
			}
		}
	}

	public void Write(BinaryWriter writer)
	{
		DataMapper.PushRange($"Structure({this})", writer);

		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				DataMapper.PushRange($"field: {field.Description}", writer);
				field.WriteWithDataMapper(writer, DataMapper);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing field {i} ({field.Description}) of {GetType().Name}", ex);
			}
			finally
			{
				DataMapper.PopRange(writer);
			}
		}

		DataMapper.PopRange(writer);
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.Description}) of {GetType().Name} (position: {reader.BaseStream.Position})", ex);
			}
		}
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		await DataMapper.PushRangeAsync($"Structure({this})", writer, cancellationToken).ConfigureAwait(false);

		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				await DataMapper.PushRangeAsync($"field: {field.Description}", writer, cancellationToken).ConfigureAwait(false);
				await field.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing field {i} ({field.Description}) of {GetType().Name}", ex);
			}
			finally
			{
				await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
			}
		}

		await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	#endregion

	#region IBinaryValue<T> Explicit Implementation

	T IBinaryField<T>.Value
	{
		get => (T) this;
		set => throw new InvalidOperationException($"DataStructures consumed as {nameof(IBinaryField<T>)} cannot be assigned a value.");
	}

	#endregion

	#region Builder Implementation

	private sealed class Builder(T structure) : DataStructureBuilder<T>(structure)
	{
		public List<DataStructureField> Build() => Fields;
	}

	#endregion
}