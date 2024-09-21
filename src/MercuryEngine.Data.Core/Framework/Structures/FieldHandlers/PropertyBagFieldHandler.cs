using MercuryEngine.Data.Core.Extensions;
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
	private readonly List<TPropertyKey>                           propertyReadOrder = [];

	public PropertyBagFieldHandler(
		IReadOnlyDictionary<string, DataStructureField> innerFields,
		IReadOnlyList<string> fieldOrder,
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		IEqualityComparer<TPropertyKey> keyComparer
	)
	{
		InnerFields = new Dictionary<string, DataStructureField>(innerFields);
		FieldOrder = [..fieldOrder];

		this.propertyKeyGenerator = propertyKeyGenerator;
		this.propertyKeyLookup = innerFields.ToDictionary(p => propertyKeyGenerator.GenerateKey(p.Key), p => p.Value, keyComparer);
		this.propertiesNotRead = new HashSet<TPropertyKey>(keyComparer);
	}

	public uint Size              => (uint) InnerFields.Values.Sum(f => f.Size);
	public bool HasMeaningfulData => InnerFields.Values.Any(f => f.HasMeaningfulData);

	public IBinaryField? Field => null;

	public bool WriteEmptyFields { get; set; }

	private Dictionary<string, DataStructureField> InnerFields { get; }
	private List<string>                           FieldOrder  { get; }
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

		for (var i = 0; i < fieldCount; i++)
		{
			var startPosition = reader.BaseStream.GetRealPosition();
			var propertyKey = this.propertyKeyGenerator.GetEmptyKey();

			propertyKey.Read(reader);

			if (!this.propertyKeyLookup.TryGetValue(propertyKey, out var field))
				throw new IOException($"Unrecognized property \"{propertyKey}\" while reading field {i} of a property bag field (position: {startPosition})");

			startPosition = reader.BaseStream.GetRealPosition();

			try
			{
				field.Read(reader);
				this.propertiesNotRead.Remove(propertyKey);
				this.propertyReadOrder.Add(propertyKey);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading sub-field {i} ({field.Description}) of a property bag field (position: {startPosition})", ex);
			}
		}

		AfterRead();
	}

	public void HandleWrite(BinaryWriter writer)
	{
		var fieldsToWrite = GetFieldsToWrite();

		try
		{
			DataMapper.PushRange($"property bag: {fieldsToWrite.Count} fields", writer);
			writer.Write(fieldsToWrite.Count);

			foreach (var (propertyKey, field) in fieldsToWrite)
			{
				propertyKey.Write(writer);

				try
				{
					DataMapper.PushRange($"property: {propertyKey}", writer);
					field.WriteWithDataMapper(writer, DataMapper);
				}
				catch (Exception ex)
				{
					throw new IOException($"An exception occurred while writing property \"{propertyKey}\" ({field.Description}) of a property bag field", ex);
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

		for (var i = 0; i < fieldCount; i++)
		{
			var startPosition = reader.BaseStream.GetRealPosition();
			var propertyKey = this.propertyKeyGenerator.GetEmptyKey();

			await propertyKey.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

			if (!this.propertyKeyLookup.TryGetValue(propertyKey, out var field))
				throw new IOException($"Unrecognized property \"{propertyKey}\" while reading field {i} of a property bag field (position: {startPosition})");

			startPosition = reader.BaseStream.GetRealPosition();

			try
			{
				await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
				this.propertiesNotRead.Remove(propertyKey);
				this.propertyReadOrder.Add(propertyKey);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading sub-field {i} ({field.Description}) of a property bag field (position: {startPosition})", ex);
			}
		}

		AfterRead();
	}

	public async Task HandleWriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
	{
		var fieldsToWrite = GetFieldsToWrite();

		try
		{
			await DataMapper.PushRangeAsync($"property bag: {fieldsToWrite.Count} fields", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync(fieldsToWrite.Count, cancellationToken).ConfigureAwait(false);

			foreach (var (propertyKey, field) in fieldsToWrite)
			{
				await propertyKey.WriteAsync(writer, cancellationToken).ConfigureAwait(false);

				try
				{
					await DataMapper.PushRangeAsync($"property: {propertyKey}", writer, cancellationToken).ConfigureAwait(false);
					await field.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new IOException($"An exception occurred while writing property \"{propertyKey}\" ({field.Description}) of a property bag field", ex);
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

	private List<(TPropertyKey, DataStructureField)> GetFieldsToWrite()
	{
		var fieldsToWrite = new List<(TPropertyKey, DataStructureField)>();
		var visitedFields = new HashSet<TPropertyKey>();

		// First, add fields that were recently read in the order in which they were read
		foreach (var propertyKey in this.propertyReadOrder)
		{
			var field = this.propertyKeyLookup[propertyKey];

			if (WriteEmptyFields || field.HasMeaningfulData)
				fieldsToWrite.Add(( propertyKey, field ));

			visitedFields.Add(propertyKey);
		}

		foreach (var fieldName in FieldOrder)
		{
			var propertyKey = this.propertyKeyGenerator.GenerateKey(fieldName);

			if (visitedFields.Contains(propertyKey))
				continue;

			var field = InnerFields[fieldName];

			if (WriteEmptyFields || field.HasMeaningfulData)
				fieldsToWrite.Add(( propertyKey, field ));
		}

		return fieldsToWrite;
	}

	private void BeforeRead()
	{
		// Ensure all properties are in the "not read" set at the start of a read operation
		foreach (var key in this.propertyKeyLookup.Keys)
			this.propertiesNotRead.Add(key);

		this.propertyReadOrder.Clear();
	}

	private void AfterRead()
	{
		// If any keys are left in the "not read" set, we need to reset those fields (if applicable)
		foreach (var key in this.propertiesNotRead)
			this.propertyKeyLookup[key].Reset();
	}
}