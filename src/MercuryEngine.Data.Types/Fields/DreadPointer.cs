using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Fields;

[PublicAPI]
public class DreadPointer<TField> : IResettableField, IDataMapperAware, IEquatable<DreadPointer<TField>>
where TField : class, ITypedDreadField
{
	private const ulong NullTypeId = 0UL;

	public DreadPointer() { }

	public DreadPointer(TField? value)
	{
		Value = value;
	}

	public TField? Value { get; set; }

	private DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public ulong InnerTypeId => Value?.TypeId ?? 0L;

	public uint GetSize(uint startPosition)
	{
		var totalSize = (uint) sizeof(ulong);

		if (Value != null)
			totalSize += Value.GetSize(startPosition + totalSize);

		return totalSize;
	}

	public void Reset()
		=> Value = null;

	public void Read(BinaryReader reader, ReadContext context)
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
		Value.Read(reader, context);
	}

	public void Write(BinaryWriter writer, WriteContext context)
	{
		try
		{
			if (Value is null)
			{
				DataMapper.PushRange("pointer: NULL", writer);

				// Write an all-zeroes type ID to indicate NULL
				writer.Write(NullTypeId);
				return;
			}

			DataMapper.PushRange($"pointer: {Value.TypeName}", writer);
			writer.Write(Value.TypeId);
			Value.WriteWithDataMapper(writer, DataMapper, context);
		}
		finally
		{
			DataMapper.PopRange(writer);
		}
	}

	public async Task ReadAsync(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken = default)
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
		await Value.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		try
		{
			if (Value is null)
			{
				await DataMapper.PushRangeAsync("pointer: NULL", writer, cancellationToken).ConfigureAwait(false);

				// Write an all-zeroes type ID to indicate NULL
				await writer.WriteAsync(NullTypeId, cancellationToken).ConfigureAwait(false);
				return;
			}

			await DataMapper.PushRangeAsync($"pointer: {Value.TypeName}", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(Value.TypeId, cancellationToken).ConfigureAwait(false);
			await Value.WriteWithDataMapperAsync(writer, DataMapper, context, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}

	#region Equality

	public bool Equals(DreadPointer<TField>? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;

		return EqualityComparer<TField?>.Default.Equals(Value, other.Value);
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;

		return Equals((DreadPointer<TField>) obj);
	}

	public override int GetHashCode()
		=> Value is null ? 0 : EqualityComparer<TField?>.Default.GetHashCode(Value);

	public static bool operator ==(DreadPointer<TField>? left, DreadPointer<TField>? right)
		=> Equals(left, right);

	public static bool operator !=(DreadPointer<TField>? left, DreadPointer<TField>? right)
		=> !Equals(left, right);

	#endregion

	#region Conversion

	public static implicit operator DreadPointer<TField>(TField? field)
		=> new(field);

	public static explicit operator TField?(DreadPointer<TField> pointer)
		=> pointer.Value;

	#endregion
}