using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TCollection">The type of item that the collection stores.</typeparam>
/// <typeparam name="TData">The data type that represents the format by which <typeparamref name="TCollection"/> is read and written.</typeparam>
public class DataStructureCollectionField<TStructure, TCollection, TData> : BaseDataStructureFieldWithProperty<TStructure, ArrayDataType<TData>, List<TCollection>?>
where TStructure : IDataStructure
where TCollection : notnull
where TData : IBinaryDataType
{
	private readonly Func<TData>                      entryFactory;
	private readonly IDataAdapter<TData, TCollection> entryDataAdapter;

	public DataStructureCollectionField(
		Func<TData> entryFactory,
		Expression<Func<TStructure, List<TCollection>?>> propertyExpression,
		IDataAdapter<TData, TCollection> entryDataAdapter
	) : base(() => new ArrayDataType<TData>(entryFactory), propertyExpression)
	{
		this.entryFactory = entryFactory;
		this.entryDataAdapter = entryDataAdapter;

		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureCollectionField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[array of {typeof(TCollection).Name}]";

	protected override ArrayDataType<TData> GetData(TStructure structure)
	{
		var collection = GetSourceCollection(structure);

		if (collection is null)
			throw new InvalidOperationException($"Source collection was null for property \"{FriendlyDescription}\" while writing data");

		var data = CreateDataType();

		data.Value.Clear();

		foreach (var item in collection)
		{
			var entry = this.entryFactory();

			this.entryDataAdapter.Put(ref entry, item);
			data.Value.Add(entry);
		}

		return data;
	}

	protected override void PutData(TStructure structure, ArrayDataType<TData> data)
	{
		var items = data.Value.Select(entry => this.entryDataAdapter.Get(entry));

		if (PropertyInfo.CanWrite)
		{
			// Store a new list
			var collection = items.ToList();

			PropertyInfo.SetValue(structure, collection);
		}
		else
		{
			// Update the existing list
			var collection = GetSourceCollection(structure);

			if (collection is null)
				throw new InvalidOperationException($"Read-only collection property \"{FriendlyDescription}\" was null on the target object when attempting to read data");

			collection.Clear();
			collection.AddRange(items);
		}
	}

	private List<TCollection>? GetSourceCollection(TStructure structure)
		=> (List<TCollection>?) PropertyInfo.GetValue(structure);
}

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TCollection">The type of data that the collection stores.</typeparam>
public class DataStructureCollectionField<TStructure, TCollection> : DataStructureCollectionField<TStructure, TCollection, TCollection>
where TStructure : IDataStructure
where TCollection : IBinaryDataType
{
	public DataStructureCollectionField(
		Func<TCollection> entryFactory,
		Expression<Func<TStructure, List<TCollection>?>> propertyExpression
	) : base(entryFactory, propertyExpression, PassthroughDataAdapter<TCollection>.Instance) { }
}