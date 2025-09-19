using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures;

[PublicAPI]
public abstract class DataStructure<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	T
> : IDataStructure, IBinaryField<T>, IDataMapperAware
where T : DataStructure<T>
{
	private static readonly ConcurrentDictionary<Type, List<DataStructureField>> FieldsByTypeCache = [];

	private readonly Lazy<List<DataStructureField>> fieldsLazy;

	protected DataStructure()
	{
		this.fieldsLazy = new Lazy<List<DataStructureField>>(GetFieldList);
	}

	[JsonIgnore]
	public uint Size => (uint) Fields.Sum(f => f.Handler.GetSize(this));

	[JsonIgnore]
	public DataMapper? DataMapper { get; set; }

	protected IEnumerable<DataStructureField> Fields => this.fieldsLazy.Value;

	public virtual void Reset()
	{
		foreach (var field in Fields)
			field.Handler.Reset(this);
	}

	#region Structure Building

	protected abstract void Describe(DataStructureBuilder<T> builder);

	private List<DataStructureField> GetFieldList()
		=> FieldsByTypeCache.GetOrAdd(GetType(), BuildFieldList, this);

	private static List<DataStructureField> BuildFieldList(Type structureType, DataStructure<T> buildingStructure)
	{
		var builder = new DataStructureBuilder<T>();

		buildingStructure.Describe(builder);

		return builder.Build();
	}

	#endregion

	#region I/O

	public void Read(BinaryReader reader, ReadContext context)
	{
		BeforeRead(context);

		ReadCore(reader, context);

		AfterRead(context);
	}

	protected virtual void ReadCore(BinaryReader reader, ReadContext context)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			var startPosition = reader.BaseStream.GetRealPosition();

			try
			{
				field.Handler.HandleRead(this, reader, context);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.Description}) of {GetType().Name} (position: {startPosition})", ex);
			}
		}
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		BeforeWrite(context);
		DataMapper.PushRange($"Structure({this})", writer);

		WriteCore(writer, context);

		DataMapper.PopRange(writer);
		AfterWrite(context);
	}

	protected virtual void WriteCore(BinaryWriter writer, WriteContext context)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				DataMapper.PushRange($"field: {field.Description}", writer);
				field.WriteWithDataMapper(this, writer, DataMapper, context);
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
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
	{
		BeforeRead(context);

		await ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		AfterRead(context);
	}

	protected virtual async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			var startPosition = reader.BaseStream.GetRealPosition();

			try
			{
				await field.Handler.HandleReadAsync(this, reader, context, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.Description}) of {GetType().Name} (position: {startPosition})", ex);
			}
		}
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		BeforeWrite(context);
		await DataMapper.PushRangeAsync($"Structure({this})", writer, cancellationToken).ConfigureAwait(false);

		await WriteAsyncCore(writer, context, cancellationToken).ConfigureAwait(false);

		await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		AfterWrite(context);
	}

	protected virtual async Task WriteAsyncCore(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		foreach (var (i, field) in Fields.Pairs())
		{
			try
			{
				await DataMapper.PushRangeAsync($"field: {field.Description}", writer, cancellationToken).ConfigureAwait(false);
				await field.WriteWithDataMapperAsync(this, writer, DataMapper, context, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
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
	}

	#endregion

	#region Hooks

	protected virtual void BeforeRead(ReadContext context) { }
	protected virtual void AfterRead(ReadContext context) { }

	protected virtual void BeforeWrite(WriteContext context) { }
	protected virtual void AfterWrite(WriteContext context) { }

	#endregion

	#region IBinaryValue<T> Explicit Implementation

	T IBinaryField<T>.Value
	{
		get => (T) this;
		set => throw new InvalidOperationException($"DataStructures consumed as {nameof(IBinaryField<T>)} cannot be assigned a value.");
	}

	#endregion
}