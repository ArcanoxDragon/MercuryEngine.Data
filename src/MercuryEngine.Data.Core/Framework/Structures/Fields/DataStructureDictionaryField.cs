﻿using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a dictionary (<see cref="Dictionary{TKey, TValue}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TKeyData">The type of key that the dictionary stores.</typeparam>
/// <typeparam name="TValueData">The type of value that the dictionary stores.</typeparam>
public class DataStructureDictionaryField<TStructure, TKeyData, TValueData> : BaseDataStructureFieldWithProperty<TStructure, ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>>, Dictionary<TKeyData, TValueData>?>
where TStructure : IDataStructure
where TKeyData : IBinaryDataType
where TValueData : IBinaryDataType
{
	private static Func<KeyValuePairDataType<TKeyData, TValueData>> CreatePairFactory(Func<TKeyData> keyDataTypeFactory, Func<TValueData> valueDataTypeFactory)
	{
		// 🍐🏭

		return () => {
			var key = keyDataTypeFactory();
			var value = valueDataTypeFactory();

			return new KeyValuePairDataType<TKeyData, TValueData>(key, value);
		};
	}

	public DataStructureDictionaryField(
		Func<TKeyData> keyDataTypeFactory,
		Func<TValueData> valueDataTypeFactory,
		Expression<Func<TStructure, Dictionary<TKeyData, TValueData>?>> propertyExpression
	) : base(() => new ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>>(CreatePairFactory(keyDataTypeFactory, valueDataTypeFactory)), propertyExpression)
	{
		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureDictionaryField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[dictionary of {typeof(TKeyData).Name},{typeof(TValueData).Name}]";

	protected override ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>> GetData(TStructure structure)
	{
		var dictionary = GetSourceDictionary(structure);

		if (dictionary is null)
			throw new InvalidOperationException($"Source dictionary was null for property \"{FriendlyDescription}\" while writing data");

		var data = CreateDataType();

		data.Value.Clear();
		data.Value.AddRange(dictionary.Select(pair => new KeyValuePairDataType<TKeyData, TValueData>(pair.Key, pair.Value)));

		return data;
	}

	protected override void PutData(TStructure structure, ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>> data)
	{
		if (PropertyInfo.CanWrite)
		{
			// Store a new dictionary
			var collection = new Dictionary<TKeyData, TValueData>();

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

	private Dictionary<TKeyData, TValueData>? GetSourceDictionary(TStructure structure)
		=> (Dictionary<TKeyData, TValueData>?) PropertyInfo.GetValue(structure);
}