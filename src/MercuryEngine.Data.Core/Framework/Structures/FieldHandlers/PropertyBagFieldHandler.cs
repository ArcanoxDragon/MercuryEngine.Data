using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Mapping;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;

/// <summary>
/// Handles reading and writing key-value dictionary where keys are bound to specific properties on a data structure.
/// </summary>
public class PropertyBagFieldHandler<TPropertyKey> : IFieldHandler, IDataMapperAware
where TPropertyKey : IBinaryField
{
	private readonly IPropertyKeyGenerator<TPropertyKey>          propertyKeyGenerator;
	private readonly Dictionary<TPropertyKey, DataStructureField> propertyKeyLookup;
	private readonly HashSet<TPropertyKey>                        propertiesNotRead;

	public PropertyBagFieldHandler(
		IDictionary<string, DataStructureField> innerFields,
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		IEqualityComparer<TPropertyKey> keyComparer
	)
	{
		InnerFields = new Dictionary<string, DataStructureField>(innerFields);

		this.propertyKeyGenerator = propertyKeyGenerator;
		this.propertyKeyLookup = innerFields.ToDictionary(p => propertyKeyGenerator.GenerateKey(p.Key), p => p.Value, keyComparer);
		this.propertiesNotRead = new HashSet<TPropertyKey>(keyComparer);
	}

	public uint          Size  => (uint) InnerFields.Values.Sum(f => f.Size);
	public IBinaryField? Field => null;

	private Dictionary<string, DataStructureField> InnerFields { get; }
	private DataMapper?                            DataMapper  { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public void Reset()
	{
		foreach (var field in InnerFields.Values)
			field.Reset();
	}

	public void HandleRead(BinaryReader reader)
	{
		BeforeRead();

		var fieldCount = reader.ReadUInt32();
		var propertyKey = this.propertyKeyGenerator.GetEmptyKey(); // Reusable instance to avoid needless allocations while reading

		for (var i = 0; i < fieldCount; i++)
		{
			propertyKey.Read(reader);

			if (!this.propertyKeyLookup.TryGetValue(propertyKey, out var field))
				throw new IOException($"Unrecognized property \"{propertyKey}\" while reading field {i} of a property bag field (position: {reader.BaseStream.Position})");

			try
			{
				field.Read(reader);
				this.propertiesNotRead.Remove(propertyKey);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading sub-field {i} ({field.Description}) of a property bag field (position: {reader.BaseStream.Position})", ex);
			}
		}

		AfterRead();
	}

	public void HandleWrite(BinaryWriter writer)
	{
		var fieldsToWrite = InnerFields.Where(f => f.Value.Size > 0).ToList();

		try
		{
			DataMapper.PushRange($"property bag: {fieldsToWrite.Count} fields", writer);
			writer.Write(fieldsToWrite.Count);

			foreach (var (propertyName, field) in fieldsToWrite)
			{
				var propertyKey = this.propertyKeyGenerator.GenerateKey(propertyName);

				propertyKey.Write(writer);

				try
				{
					DataMapper.PushRange($"property: {propertyName}", writer);
					field.WriteWithDataMapper(writer, DataMapper);
				}
				catch (Exception ex)
				{
					throw new IOException($"An exception occurred while writing property \"{propertyName}\" ({field.Description}) of a property bag field", ex);
				}
				finally
				{
					DataMapper.PopRange(writer);
				}
			}
		}
		finally
		{
			DataMapper.PopRange(writer);
		}
	}

	public async Task HandleReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
	{
		BeforeRead();

		var fieldCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		var propertyKey = this.propertyKeyGenerator.GetEmptyKey(); // Reusable instance to avoid needless allocations while reading

		for (var i = 0; i < fieldCount; i++)
		{
			await propertyKey.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

			if (!this.propertyKeyLookup.TryGetValue(propertyKey, out var field))
				throw new IOException($"Unrecognized property \"{propertyKey}\" while reading field {i} of a property bag field (position: {reader.BaseStream.Position})");

			try
			{
				await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
				this.propertiesNotRead.Remove(propertyKey);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading sub-field {i} ({field.Description}) of a property bag field (position: {reader.BaseStream.Position})", ex);
			}
		}

		AfterRead();
	}

	public async Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		var fieldsToWrite = InnerFields.Where(f => f.Value.Size > 0).ToList();

		try
		{
			await DataMapper.PushRangeAsync($"property bag: {fieldsToWrite.Count} fields", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(fieldsToWrite.Count, cancellationToken).ConfigureAwait(false);

			foreach (var (propertyName, field) in fieldsToWrite)
			{
				var propertyKey = this.propertyKeyGenerator.GenerateKey(propertyName);

				await propertyKey.WriteAsync(writer, cancellationToken).ConfigureAwait(false);

				try
				{
					await DataMapper.PushRangeAsync($"property: {propertyName}", writer, cancellationToken).ConfigureAwait(false);
					await field.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new IOException($"An exception occurred while writing property \"{propertyName}\" ({field.Description}) of a property bag field", ex);
				}
				finally
				{
					await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
				}
			}
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}

	private void BeforeRead()
	{
		// Ensure all properties are in the "not read" set at the start of a read operation
		foreach (var key in this.propertyKeyLookup.Keys)
			this.propertiesNotRead.Add(key);
	}

	private void AfterRead()
	{
		// If any keys are left in the "not read" set, we need to reset those fields (if applicable)
		foreach (var key in this.propertiesNotRead)
			this.propertyKeyLookup[key].Reset();

		this.propertiesNotRead.Clear();
	}
}