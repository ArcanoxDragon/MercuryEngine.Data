using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing a property that reads and writes a direct <see cref="IBinaryField"/> instance as a pointer to heap data.
/// </summary>
public class PointerPropertyFieldHandler<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TOwner,
	TField
>(PropertyInfo property, Func<TOwner, TField> fieldFactory, uint startByteAlignment = 0, uint endByteAlignment = 0, bool unique = false) : IFieldHandler
where TOwner : IDataStructure
where TField : IBinaryField
{
	private readonly Func<TOwner, TField?>   getter = ReflectionUtility.GetGetter<TOwner, TField?>(property);
	private readonly Action<TOwner, TField?> setter = ReflectionUtility.GetSetter<TOwner, TField?>(property);

	public uint GetSize(IDataStructure dataStructure)
		=> sizeof(ulong); // Just the pointer address

	public IBinaryField? GetField(IDataStructure dataStructure)
		=> this.getter((TOwner) dataStructure);

	public void SetField(IDataStructure dataStructure, TField? field)
		=> this.setter((TOwner) dataStructure, field);

	public void Reset(IDataStructure dataStructure)
		=> ( GetField(dataStructure) as IResettableField )?.Reset();

	public void HandleRead(IDataStructure dataStructure, BinaryReader reader, ReadContext context)
	{
		var address = reader.ReadUInt64();

		if (address > 0)
		{
			if (context.HeapManager.TryGetField(address, out var field) && field is TField typedField)
			{
				SetField(dataStructure, typedField);
			}
			else
			{
				field = GetOrCreateField(dataStructure);

				if (!reader.BaseStream.CanSeek)
					throw new NotSupportedException("Cannot read pointer field because the underlying stream does not support seeking");
				if (address >= (ulong) reader.BaseStream.Length)
					throw new IOException("Target address of field was beyond the end of the stream");

				// Read field from the appropriate address and then restore the stream position
				var prevPosition = reader.BaseStream.Position;

				reader.BaseStream.Seek((long) address, SeekOrigin.Begin);
				field.Read(reader, context);
				context.HeapManager.Register(address, field);
				reader.BaseStream.Position = prevPosition;
			}
		}
		else
		{
			SetField(dataStructure, default);
		}
	}

	public void HandleWrite(IDataStructure dataStructure, BinaryWriter writer, WriteContext context)
	{
		var address = PrepareAddressForWrite(dataStructure, context);
		var dataMapper = ( GetField(dataStructure) as IDataMapperAware )?.DataMapper;

		dataMapper.PushRange($"Pointer to 0x{address:X16}", writer);
		writer.Write(address);
		// Field data will be written by HeapManager
		dataMapper.PopRange(writer);
	}

	public async Task HandleReadAsync(IDataStructure dataStructure, AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		var address = await reader.ReadUInt64Async(cancellationToken).ConfigureAwait(false);

		if (address > 0)
		{
			if (context.HeapManager.TryGetField(address, out var field) && field is TField typedField)
			{
				SetField(dataStructure, typedField);
			}
			else
			{
				field = GetOrCreateField(dataStructure);

				if (!reader.BaseStream.CanSeek)
					throw new NotSupportedException("Cannot read pointer field because the underlying stream does not support seeking");
				if (address >= (ulong) reader.BaseStream.Length)
					throw new IOException($"Target address of field was beyond the end of the stream");

				// Read field from the appropriate address and then restore the stream position
				var prevPosition = reader.BaseStream.Position;

				reader.BaseStream.Seek((long) address, SeekOrigin.Begin);
				await field.ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
				context.HeapManager.Register(address, field);
				reader.BaseStream.Position = prevPosition;
			}
		}
		else
		{
			SetField(dataStructure, default);
		}
	}

	public async Task HandleWriteAsync(IDataStructure dataStructure, AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken)
	{
		var address = PrepareAddressForWrite(dataStructure, context);
		var dataMapper = ( GetField(dataStructure) as IDataMapperAware )?.DataMapper;

		await dataMapper.PushRangeAsync($"Pointer to 0x{address:X16}", writer, cancellationToken).ConfigureAwait(false);
		await writer.WriteAsync(address, cancellationToken).ConfigureAwait(false);
		// Field data will be written by HeapManager
		await dataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
	}

	private TField GetOrCreateField(IDataStructure dataStructure)
	{
		if (GetField(dataStructure) is not TField field)
		{
			field = fieldFactory((TOwner) dataStructure);
			SetField(dataStructure, field);
		}

		return field;
	}

	private ulong PrepareAddressForWrite(IDataStructure dataStructure, WriteContext context)
	{
		var field = GetField(dataStructure);

		if (field is null or { Size: 0 })
			// Write "0" for "null pointer"
			return 0;

		// Allocate space for field data

		if (unique)
			// Always allocate
			return context.HeapManager.Allocate(field, startByteAlignment, endByteAlignment);

		return context.HeapManager.GetAddressOrAllocate(field, startByteAlignment, endByteAlignment);
	}
}