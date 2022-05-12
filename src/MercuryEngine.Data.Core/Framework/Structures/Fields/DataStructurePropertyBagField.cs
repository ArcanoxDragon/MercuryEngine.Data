using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing a collection of named properties that may be stored in any order in a binary format.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the properties to be read/written.</typeparam>
/// <typeparam name="TPropertyKey">The type of key used to identify properties when written to a binary format.</typeparam>
public class DataStructurePropertyBagField<TStructure, TPropertyKey> : IDataStructureField<TStructure>
where TStructure : IDataStructure
where TPropertyKey : IBinaryDataType
{
	private readonly IPropertyKeyGenerator<TPropertyKey>                       propertyKeyGenerator;
	private readonly Func<TPropertyKey>                                        emptyPropertyKeyFactory;
	private readonly Dictionary<string, IDataStructureField<TStructure>>       innerFields;
	private readonly Dictionary<TPropertyKey, IDataStructureField<TStructure>> propertyKeyLookup;

	public DataStructurePropertyBagField(
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		Func<TPropertyKey> emptyPropertyKeyFactory,
		IReadOnlyDictionary<string, IDataStructureField<TStructure>> innerFields,
		IEqualityComparer<TPropertyKey> keyEqualityComparer
	)
	{
		this.propertyKeyGenerator = propertyKeyGenerator;
		this.emptyPropertyKeyFactory = emptyPropertyKeyFactory;
		this.innerFields = new Dictionary<string, IDataStructureField<TStructure>>(innerFields);
		this.propertyKeyLookup = this.innerFields.ToDictionary(pair => propertyKeyGenerator.GenerateKey(pair.Key), pair => pair.Value, keyEqualityComparer);
	}

	public DataStructurePropertyBagField(
		IPropertyKeyGenerator<TPropertyKey> propertyKeyGenerator,
		Func<TPropertyKey> emptyPropertyKeyFactory,
		IReadOnlyDictionary<string, IDataStructureField<TStructure>> innerFields
	) : this(propertyKeyGenerator, emptyPropertyKeyFactory, innerFields, EqualityComparer<TPropertyKey>.Default) { }

	public string FriendlyDescription => $"<property bag: {this.innerFields.Count} properties>";

	protected IEnumerable<IDataStructureField<TStructure>> Fields => this.innerFields.Values;

	public void ClearData(TStructure structure)
	{
		foreach (var field in Fields)
			field.ClearData(structure);
	}

	public bool HasData(TStructure structure)
		=> Fields.Any(f => f.HasData(structure));

	public uint GetSize(TStructure structure)
		=> (uint) Fields.Where(f => f.HasData(structure)).Sum(f => f.GetSize(structure));

	public void Read(TStructure structure, BinaryReader reader)
	{
		foreach (var field in Fields)
			field.ClearData(structure);

		var fieldCount = reader.ReadUInt32();
		var propertyKey = this.emptyPropertyKeyFactory(); // Reusable instance for reading each key in turn

		for (var i = 0; i < fieldCount; i++)
		{
			propertyKey.Read(reader);

			if (!this.propertyKeyLookup.TryGetValue(propertyKey, out var field))
				throw new IOException($"Unrecognized property \"{propertyKey}\" while reading field {i} of {GetType().Name}");

			try
			{
				field.Read(structure, reader);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while reading field {i} ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}

	public void Write(TStructure structure, BinaryWriter writer)
	{
		var fieldsToWrite = this.innerFields.Where(f => f.Value.HasData(structure)).ToList();

		writer.Write(fieldsToWrite.Count);

		foreach (var (propertyName, field) in fieldsToWrite)
		{
			var propertyKey = this.propertyKeyGenerator.GenerateKey(propertyName);

			propertyKey.Write(writer);

			try
			{
				field.Write(structure, writer);
			}
			catch (Exception ex)
			{
				throw new IOException($"An exception occurred while writing property \"{propertyName}\" ({field.FriendlyDescription}) of {GetType().Name}", ex);
			}
		}
	}
}