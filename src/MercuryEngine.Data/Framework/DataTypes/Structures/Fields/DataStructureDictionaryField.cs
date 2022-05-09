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
where TStructure : DataStructure<TStructure>
where TKeyData : IBinaryDataType, new()
where TValueData : IBinaryDataType, new()
{
	private readonly PropertyInfo propertyInfo;

	public DataStructureDictionaryField(
		TStructure structure,
		Expression<Func<TStructure, Dictionary<TKeyData, TValueData>>> propertyExpression
	) : base(structure)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

		if (!this.propertyInfo.CanRead || this.propertyInfo.CanWrite)
			throw new ArgumentException("A property must be read-only in order to be used in a DataStructureDictionaryField");

		if (!typeof(Dictionary<TKeyData, TValueData>).IsAssignableFrom(this.propertyInfo.PropertyType))
			throw new ArgumentException($"A property must be of type {nameof(Dictionary<TKeyData, TValueData>)} in order to be used in a DataStructureDictionaryField");

		Data = new ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>>(() => new KeyValuePairDataType<TKeyData, TValueData>(new TKeyData(), new TValueData()));
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[dictionary of {typeof(TKeyData).Name},{typeof(TValueData).Name}]";

	protected override ArrayDataType<KeyValuePairDataType<TKeyData, TValueData>> Data { get; }

	protected Dictionary<TKeyData, TValueData> SourceDictionary
	{
		get
		{
			var propertyValue = this.propertyInfo.GetValue(Structure);

			if (propertyValue is null)
				throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

			if (propertyValue is not Dictionary<TKeyData, TValueData> dictionary)
				throw new InvalidOperationException();

			return dictionary;
		}
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		// Synchronize back to the property
		SourceDictionary.Clear();

		foreach (var (key, value) in Data.Value)
			SourceDictionary.Add(key, value);
	}

	public override void Write(BinaryWriter writer)
	{
		// Copy dictionary entries into data type
		Data.Value = SourceDictionary.Select(pair => new KeyValuePairDataType<TKeyData, TValueData>(pair.Key, pair.Value)).ToList();

		base.Write(writer);
	}
}