using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Framework.Structures.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public abstract class DataStructure<T> : IDataStructure, IBinaryField<T>, IDataMapperAware
where T : DataStructure<T>
{
	private readonly Lazy<List<IDataStructureField<T>>> fieldsLazy;

	protected DataStructure()
	{
		this.fieldsLazy = new Lazy<List<IDataStructureField<T>>>(BuildFields);
	}

	public uint Size => (uint) Fields.Sum(f => f.GetSize((T) this));

	public DataMapper? DataMapper { get; set; }

	protected IEnumerable<IDataStructureField<T>> Fields => this.fieldsLazy.Value;

	protected abstract void Describe(DataStructureBuilder<T> builder);

	#region I/O

	public void Read(BinaryReader reader)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				field.Read((T) this, reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name} (position: {reader.BaseStream.Position})", ex);
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
				DataMapper.PushRange($"field: {field.FriendlyDescription}", writer);
				field.WriteWithDataMapper((T) this, writer, DataMapper);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
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
				await field.ReadAsync((T) this, reader, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name} (position: {reader.BaseStream.Position})", ex);
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
				await DataMapper.PushRangeAsync($"field: {field.FriendlyDescription}", writer, cancellationToken).ConfigureAwait(false);
				await field.WriteWithDataMapperAsync((T) this, writer, DataMapper, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
			finally
			{
				await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
			}
		}

		await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	#endregion

	private List<IDataStructureField<T>> BuildFields()
	{
		var builder = new Builder();

		Describe(builder);

		return builder.Build();
	}

	#region IBinaryValue<T> Explicit Implementation

	T IBinaryField<T>.Value
	{
		get => (T) this;
		set => throw new InvalidOperationException($"DataStructures consumed as {nameof(IBinaryField<T>)} cannot be assigned a value.");
	}

	#endregion

	#region Builder Implementation

	private sealed class Builder : DataStructureBuilder<T>
	{
		public List<IDataStructureField<T>> Build() => Fields;
	}

	#endregion
}