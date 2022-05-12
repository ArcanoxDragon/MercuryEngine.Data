using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TCollection">The type of item that the collection stores.</typeparam>
/// <typeparam name="TData">The data type that represents the format by which <typeparamref name="TCollection"/> is read and written.</typeparam>
public class DataStructureCollectionField<TStructure, TCollection, TData> : BaseDataStructureField<TStructure, ArrayDataType<TData>>
where TStructure : DataStructure<TStructure>
where TCollection : notnull
where TData : IBinaryDataType
{
	private readonly Func<TData>                      entryFactory;
	private readonly PropertyInfo                     propertyInfo;
	private readonly IDataAdapter<TData, TCollection> entryDataAdapter;

	public DataStructureCollectionField(
		Func<TData> entryFactory,
		Expression<Func<TStructure, List<TCollection>>> propertyExpression,
		IDataAdapter<TData, TCollection> entryDataAdapter
	) : base(() => new ArrayDataType<TData>(entryFactory))
	{
		this.entryFactory = entryFactory;
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);
		this.entryDataAdapter = entryDataAdapter;

		if (!this.propertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureCollectionField");
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[array of {typeof(TCollection).Name}]";

	protected override ArrayDataType<TData> GetData(TStructure structure)
	{
		var collection = GetSourceCollection(structure);
		var data = CreateDataType();

		data.Value.Clear();

		foreach (var item in collection)
		{
			var entry = this.entryFactory();

			this.entryDataAdapter.Put(entry, item);
			data.Value.Add(entry);
		}

		return data;
	}

	protected override void PutData(TStructure structure, ArrayDataType<TData> data)
	{
		var items = data.Value.Select(entry => this.entryDataAdapter.Get(entry));

		if (this.propertyInfo.CanWrite)
		{
			// Store a new list
			var collection = items.ToList();

			this.propertyInfo.SetValue(structure, collection);
		}
		else
		{
			// Update the existing list
			var collection = GetSourceCollection(structure);

			collection.Clear();
			collection.AddRange(items);
		}
	}

	private List<TCollection> GetSourceCollection(TStructure structure)
	{
		var collection = (List<TCollection>?) this.propertyInfo.GetValue(structure);

		if (collection is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

		return collection;
	}
}

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TCollection">The type of data that the collection stores.</typeparam>
public class DataStructureCollectionField<TStructure, TCollection> : BaseDataStructureField<TStructure, ArrayDataType<TCollection>>
where TStructure : IDataStructure
where TCollection : IBinaryDataType
{
	private readonly PropertyInfo propertyInfo;

	public DataStructureCollectionField(
		Func<TCollection> entryFactory,
		Expression<Func<TStructure, List<TCollection>>> propertyExpression
	) : base(() => new ArrayDataType<TCollection>(entryFactory))
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

		if (!this.propertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureCollectionField");
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[array of {typeof(TCollection).Name}]";

	protected override ArrayDataType<TCollection> GetData(TStructure structure)
	{
		var collection = GetSourceCollection(structure);
		var data = CreateDataType();

		data.Value.Clear();
		data.Value.AddRange(collection);

		return data;
	}

	protected override void PutData(TStructure structure, ArrayDataType<TCollection> data)
	{
		if (this.propertyInfo.CanWrite)
		{
			// Store a new list
			var collection = new List<TCollection>(data.Value);

			this.propertyInfo.SetValue(structure, collection);
		}
		else
		{
			// Update the existing list
			var collection = GetSourceCollection(structure);

			collection.Clear();
			collection.AddRange(data.Value);
		}
	}

	private List<TCollection> GetSourceCollection(TStructure structure)
	{
		var collection = (List<TCollection>?) this.propertyInfo.GetValue(structure);

		if (collection is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

		return collection;
	}
}