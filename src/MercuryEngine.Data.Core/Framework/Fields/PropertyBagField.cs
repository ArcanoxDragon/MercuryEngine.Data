using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Framework.Mapping;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public interface IPropertyBagField : IResettableField, IDataMapperAware
{
	IBinaryField? Get(string propertyName);

	void Set(string propertyName, IBinaryField field);

	TValue? GetValue<TValue>(string propertyName, TValue? defaultValue = default)
	where TValue : class;

	TValue? GetValue<TValue>(string propertyName, TValue? defaultValue = default)
	where TValue : struct;

	void SetValue<TValue>(string propertyName, TValue value)
	where TValue : notnull;

	void ClearProperty(string propertyName);
}

[PublicAPI]
public class PropertyBagField<TPropertyKey> : IPropertyBagField
where TPropertyKey : IBinaryField
{
	private readonly object lockObject = new();

	private readonly OrderedMultiDictionary<string, IBinaryField> values;
	private readonly Dictionary<string, Func<IBinaryField>>       fieldDefinitions;
	private readonly Dictionary<TPropertyKey, string>             propertyKeyReverseLookup;
	private readonly IPropertyKeyTranslator<TPropertyKey>         propertyKeyTranslator;

	public PropertyBagField(
		IReadOnlyDictionary<string, Func<IBinaryField>> fieldDefinitions,
		IPropertyKeyTranslator<TPropertyKey> propertyKeyTranslator,
		IEqualityComparer<TPropertyKey> keyComparer
	)
	{
		this.values = new OrderedMultiDictionary<string, IBinaryField>();
		this.fieldDefinitions = new Dictionary<string, Func<IBinaryField>>(fieldDefinitions);
		this.propertyKeyReverseLookup = fieldDefinitions.Keys.ToDictionary(propertyKeyTranslator.TranslateKey, p => p, keyComparer);
		this.propertyKeyTranslator = propertyKeyTranslator;
	}

	public PropertyBagField(
		PropertyBagFieldBuilder builder,
		IPropertyKeyTranslator<TPropertyKey> propertyKeyTranslator,
		IEqualityComparer<TPropertyKey> keyComparer)
		: this(builder.Fields, propertyKeyTranslator, keyComparer) { }

	protected PropertyBagField(PropertyBagField<TPropertyKey> other)
	{
		this.values = new OrderedMultiDictionary<string, IBinaryField>();
		this.fieldDefinitions = other.fieldDefinitions;
		this.propertyKeyReverseLookup = other.propertyKeyReverseLookup;
		this.propertyKeyTranslator = other.propertyKeyTranslator;
	}

	[JsonIgnore]
	public uint Size => sizeof(uint) + (uint) this.values.Sum(
		pair => this.propertyKeyTranslator.GetKeySize(pair.Key) + pair.Value.Size
	);

	protected DataMapper? DataMapper { get; set; }

	DataMapper? IDataMapperAware.DataMapper
	{
		get => DataMapper;
		set => DataMapper = value;
	}

	public virtual PropertyBagField<TPropertyKey> Clone() => new(this);

	#region Value Getters/Setters

	public IBinaryField? Get(string propertyName)
	{
		if (!this.fieldDefinitions.ContainsKey(propertyName))
			throw UnrecognizedPropertyException(propertyName);

		return this.values.TryGetValue(propertyName, out var field) ? field : null;
	}

	public void Set(string propertyName, IBinaryField field)
	{
		if (!this.fieldDefinitions.ContainsKey(propertyName))
			throw UnrecognizedPropertyException(propertyName);

		this.values[propertyName] = field;
	}

	[return: NotNullIfNotNull(nameof(defaultValue))]
	public TValue? GetValue<TValue>(string propertyName, TValue? defaultValue = default)
	where TValue : class
	{
		if (!this.fieldDefinitions.ContainsKey(propertyName))
			throw UnrecognizedPropertyException(propertyName);

		if (!this.values.TryGetValue(propertyName, out var field))
			return defaultValue;

		if (field is not IBinaryField<TValue> valueField)
			throw NotValueFieldException<TValue>(propertyName, field.GetType());

		return valueField.Value;
	}

	[return: NotNullIfNotNull(nameof(defaultValue))]
	public TValue? GetValue<TValue>(string propertyName, TValue? defaultValue = default)
	where TValue : struct
	{
		if (!this.fieldDefinitions.ContainsKey(propertyName))
			throw UnrecognizedPropertyException(propertyName);

		if (!this.values.TryGetValue(propertyName, out var field))
			return defaultValue;

		if (field is not IBinaryField<TValue> valueField)
			throw NotValueFieldException<TValue>(propertyName, field.GetType());

		return valueField.Value;
	}

	public void SetValue<TValue>(string propertyName, TValue value)
	where TValue : notnull
	{
		if (!this.fieldDefinitions.TryGetValue(propertyName, out var fieldFactory))
			throw UnrecognizedPropertyException(propertyName);

		var field = fieldFactory();

		if (field is not IBinaryField<TValue> valueField)
			throw NotValueFieldException<TValue>(propertyName, field.GetType());

		valueField.Value = value;
		this.values[propertyName] = valueField;
	}

	public void ClearProperty(string propertyName)
		=> this.values.RemoveAll(propertyName);

	#endregion

	#region I/O

	public void Read(BinaryReader reader)
	{
		BeforeRead();

		var fieldCount = reader.ReadUInt32();
		var propertyKey = this.propertyKeyTranslator.GetEmptyKey(); // One single instance to avoid lots of allocations

		for (var i = 0; i < fieldCount; i++)
		{
			var startPosition = reader.BaseStream.GetRealPosition();

			propertyKey.Read(reader);

			if (!this.propertyKeyReverseLookup.TryGetValue(propertyKey, out var propertyName) ||
				!this.fieldDefinitions.TryGetValue(propertyName, out var fieldFactory))
				throw UnknownFieldException(propertyKey, i, startPosition);

			startPosition = reader.BaseStream.GetRealPosition();

			var field = fieldFactory();

			try
			{
				field.Read(reader);
				this.values.Add(propertyName, field);
			}
			catch (Exception ex)
			{
				throw FieldReadException(propertyName, i, field, startPosition, ex);
			}
		}
	}

	public void Write(BinaryWriter writer)
	{
		try
		{
			DataMapper.PushRange($"property bag: {this.values.Count} fields", writer);
			writer.Write((uint) this.values.Count);

			var i = 0;

			foreach (var (propertyName, field) in this.values)
			{
				var propertyKey = this.propertyKeyTranslator.TranslateKey(propertyName);

				propertyKey.Write(writer);

				try
				{
					DataMapper.PushRange($"property: {propertyName}", writer);
					field.WriteWithDataMapper(writer, DataMapper);
				}
				catch (Exception ex)
				{
					throw FieldWriteException(propertyName, i, field, ex);
				}
				finally
				{
					DataMapper.PopRange(writer);
				}

				i++;
			}
		}
		finally
		{
			DataMapper.PopRange(writer);
		}
	}

	public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
	{
		BeforeRead();

		var fieldCount = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
		var propertyKey = this.propertyKeyTranslator.GetEmptyKey(); // One single instance to avoid lots of allocations

		for (var i = 0; i < fieldCount; i++)
		{
			var startPosition = reader.BaseStream.GetRealPosition();

			await propertyKey.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

			if (!this.propertyKeyReverseLookup.TryGetValue(propertyKey, out var propertyName) ||
				!this.fieldDefinitions.TryGetValue(propertyName, out var fieldFactory))
				throw UnknownFieldException(propertyKey, i, startPosition);

			startPosition = reader.BaseStream.GetRealPosition();

			var field = fieldFactory();

			try
			{
				await field.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
				this.values.Add(propertyName, field);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw FieldReadException(propertyName, i, field, startPosition, ex);
			}
		}
	}

	public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
	{
		try
		{
			await DataMapper.PushRangeAsync($"property bag: {this.values.Count} fields", writer, cancellationToken).ConfigureAwait(false);
			await writer.WriteAsync((uint) this.values.Count, cancellationToken).ConfigureAwait(false);

			var i = 0;

			foreach (var (propertyName, field) in this.values)
			{
				var propertyKey = this.propertyKeyTranslator.TranslateKey(propertyName);

				await propertyKey.WriteAsync(writer, cancellationToken).ConfigureAwait(false);

				try
				{
					await DataMapper.PushRangeAsync($"property: {propertyName}", writer, cancellationToken).ConfigureAwait(false);
					await field.WriteWithDataMapperAsync(writer, DataMapper, cancellationToken).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw FieldWriteException(propertyName, i, field, ex);
				}
				finally
				{
					await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
				}

				i++;
			}
		}
		finally
		{
			await DataMapper.PopRangeAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}

	public void Reset()
		=> this.values.Clear();

	#endregion

	#region Private Methods

	private void BeforeRead()
		=> Reset();

	private static IOException UnknownFieldException(TPropertyKey propertyKey, int index, long startPosition)
		=> new($"Unrecognized property \"{propertyKey}\" while reading field {index} of a property bag field (position: {startPosition})");

	private static IOException FieldReadException(string propertyName, int index, IBinaryField field, long startPosition, Exception innerException)
		=> new($"An exception occurred while reading sub-field {index} ({propertyName}: {field}) " +
			   $"of a property bag field (position: {startPosition})", innerException);

	private static IOException FieldWriteException(string propertyName, int index, IBinaryField field, Exception innerException)
		=> new($"An exception occurred while writing sub-field {index} ({propertyName}: {field}) " +
			   $"of a property bag field", innerException);

	private static ArgumentException UnrecognizedPropertyException(string propertyName)
		=> new($"Unrecognized property name: {propertyName}");

	private static InvalidOperationException NotValueFieldException<TValue>(string propertyName, Type fieldType)
	where TValue : notnull
		=> new($"Property \"{propertyName}\" has field type \"{fieldType.Name}\", which does not implement {nameof(IBinaryField<TValue>)}");

	#endregion
}

[PublicAPI]
public static class PropertyBagField
{
	public static PropertyBagField<TPropertyKey> Create<TPropertyKey>(
		Action<PropertyBagFieldBuilder> defineFields,
		IPropertyKeyTranslator<TPropertyKey> propertyKeyTranslator)
	where TPropertyKey : IBinaryField
		=> Create(defineFields, propertyKeyTranslator, EqualityComparer<TPropertyKey>.Default);

	public static PropertyBagField<TPropertyKey> Create<TPropertyKey>(
		Action<PropertyBagFieldBuilder> defineFields,
		IPropertyKeyTranslator<TPropertyKey> propertyKeyTranslator,
		IEqualityComparer<TPropertyKey> keyComparer)
	where TPropertyKey : IBinaryField
	{
		PropertyBagFieldBuilder builder = new();

		defineFields(builder);

		return new PropertyBagField<TPropertyKey>(builder, propertyKeyTranslator, keyComparer);
	}
}