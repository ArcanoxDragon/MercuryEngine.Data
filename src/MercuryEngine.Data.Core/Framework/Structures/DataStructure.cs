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
where T : DataStructure<T>
{
	private readonly Lazy<List<DataStructureField>> fieldsLazy;

	protected DataStructure()
	{
		this.fieldsLazy = new Lazy<List<DataStructureField>>(BuildFieldList);
	}

	[JsonIgnore]
	public uint Size => (uint) Fields.Sum(f => f.Size);

	[JsonIgnore]
	public virtual bool HasMeaningfulData => Fields.Any(f => f.HasMeaningfulData);

	[JsonIgnore]
	public DataMapper? DataMapper { get; set; }

	protected IEnumerable<DataStructureField> Fields => this.fieldsLazy.Value;

	public void Reset()
	{
		foreach (var field in Fields)
			field.Reset();
	}

	#region Structure Building

	protected abstract void Describe(DataStructureBuilder<T> builder);

	private List<DataStructureField> BuildFieldList()
	{
		var builder = new DataStructureBuilder<T>((T) this);

		Describe(builder);

		return builder.Build();
	}

	#endregion

	#region I/O

	public void Read(BinaryReader reader)
	{
		BeforeRead();

		foreach (var (i, field) in Fields.Pairs())
		{
			var startPosition = reader.BaseStream.GetRealPosition();

			try
			{
				field.Read(reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.Description}) of {GetType().Name} (position: {startPosition})", ex);
			}
		}

		AfterRead();
	}

	public void Write(BinaryWriter writer)
	{
		BeforeWrite();
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
		AfterWrite();
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		BeforeRead();

		foreach (var (i, field) in Fields.Pairs())
		{
			var startPosition = reader.BaseStream.GetRealPosition();

			try
			{
				await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.Description}) of {GetType().Name} (position: {startPosition})", ex);
			}
		}

		AfterRead();
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		BeforeWrite();
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
		AfterWrite();
	}

	#endregion

	#region Hooks

	protected virtual void BeforeRead() { }
	protected virtual void AfterRead() { }

	protected virtual void BeforeWrite() { }
	protected virtual void AfterWrite() { }

	#endregion

	#region IBinaryValue<T> Explicit Implementation

	T IBinaryField<T>.Value
	{
		get => (T) this;
		set => throw new InvalidOperationException($"DataStructures consumed as {nameof(IBinaryField<T>)} cannot be assigned a value.");
	}

	#endregion
}