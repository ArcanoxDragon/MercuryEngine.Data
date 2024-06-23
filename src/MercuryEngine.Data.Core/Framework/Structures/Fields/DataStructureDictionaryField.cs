using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a dictionary (<see cref="Dictionary{TKey, TValue}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TKeyField">The type of key that the dictionary stores.</typeparam>
/// <typeparam name="TValueField">The type of value that the dictionary stores.</typeparam>
public class DataStructureDictionaryField<TStructure, TKeyField, TValueField> : BaseDataStructureFieldWithProperty<TStructure, DictionaryField<TKeyField, TValueField>, Dictionary<TKeyField, TValueField>?>
where TStructure : IDataStructure
where TKeyField : IBinaryField
where TValueField : IBinaryField
{
	private static Func<KeyValuePairField<TKeyField, TValueField>> CreatePairFactory(Func<TKeyField> keyFieldFactory, Func<TValueField> valueFieldFactory)
	{
		// 🍐🏭

		return () => {
			var key = keyFieldFactory();
			var value = valueFieldFactory();

			return new KeyValuePairField<TKeyField, TValueField>(key, value);
		};
	}

	public DataStructureDictionaryField(
		Func<TKeyField> keyFieldFactory,
		Func<TValueField> valueFieldFactory,
		Expression<Func<TStructure, Dictionary<TKeyField, TValueField>?>> propertyExpression
	) : base(() => new DictionaryField<TKeyField, TValueField>(CreatePairFactory(keyFieldFactory, valueFieldFactory)), propertyExpression)
	{
		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureDictionaryField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[dictionary of {typeof(TKeyField).Name},{typeof(TValueField).Name}]";

	protected override DictionaryField<TKeyField, TValueField> GetFieldForStorage(TStructure structure)
	{
		var dictionary = GetSourceDictionary(structure);

		if (dictionary is null)
			throw new InvalidOperationException($"Source dictionary was null for property \"{FriendlyDescription}\" while writing data");

		var field = CreateFieldInstance();

		field.Value.Clear();
		field.Value.AddRange(dictionary.Select(pair => new KeyValuePairField<TKeyField, TValueField>(pair.Key, pair.Value)));

		return field;
	}

	protected override void LoadFieldFromStorage(TStructure structure, DictionaryField<TKeyField, TValueField> data)
	{
		if (PropertyInfo.CanWrite)
		{
			// Store a new dictionary
			var collection = new Dictionary<TKeyField, TValueField>();

			foreach (var (key, value) in data.Value)
				collection[key] = value;

			PropertyInfo.SetValue(structure, collection);
		}
		else
		{
			// Update the existing dictionary
			var dictionary = GetSourceDictionary(structure);

			if (dictionary is null)
				throw new InvalidOperationException($"Read-only dictionary property \"{FriendlyDescription}\" was null on the target object when attempting to read data");

			dictionary.Clear();

			foreach (var (key, value) in data.Value)
				dictionary.Add(key, value);
		}
	}

	private Dictionary<TKeyField, TValueField>? GetSourceDictionary(TStructure structure)
		=> (Dictionary<TKeyField, TValueField>?) PropertyInfo.GetValue(structure);
}