using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a dictionary (<see cref="Dictionary{TKey, TValue}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TKeyData">The type of key that the dictionary stores.</typeparam>
/// <typeparam name="TValueData">The type of value that the dictionary stores.</typeparam>
public class DataStructureDictionaryField<TStructure, TKeyData, TValueData> : BaseDataStructureField<TStructure, ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>>>
where TStructure : IDataStructure
where TKeyData : IBinaryDataType
where TValueData : IBinaryDataType
{
	private readonly PropertyInfo propertyInfo;

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
		Expression<Func<TStructure, Dictionary<TKeyData, TValueData>>> propertyExpression
	) : base(() => new ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>>(CreatePairFactory(keyDataTypeFactory, valueDataTypeFactory)))
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

		if (!this.propertyInfo.CanRead || this.propertyInfo.CanWrite)
			throw new ArgumentException("A property must be read-only in order to be used in a DataStructureDictionaryField");
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[dictionary of {typeof(TKeyData).Name},{typeof(TValueData).Name}]";

	protected override ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>> GetData(TStructure structure)
	{
		var dictionary = GetSourceDictionary(structure);
		var data = CreateDataType();

		data.Value.Clear();
		data.Value.AddRange(dictionary.Select(pair => new KeyValuePairDataType<TKeyData, TValueData>(pair.Key, pair.Value)));

		return data;
	}

	protected override void PutData(TStructure structure, ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>> data)
	{
		if (this.propertyInfo.CanWrite)
		{
			// Store a new dictionary
			var collection = data.Value.ToDictionary(pair => pair.Key, pair => pair.Value);

			this.propertyInfo.SetValue(structure, collection);
		}
		else
		{
			// Update the existing dictionary
			var dictionary = GetSourceDictionary(structure);

			dictionary.Clear();

			foreach (var (key, value) in data.Value)
				dictionary.Add(key, value);
		}
	}

	private Dictionary<TKeyData, TValueData> GetSourceDictionary(TStructure structure)
	{
		var dictionary = (Dictionary<TKeyData, TValueData>?) this.propertyInfo.GetValue(structure);

		if (dictionary is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

		return dictionary;
	}
}