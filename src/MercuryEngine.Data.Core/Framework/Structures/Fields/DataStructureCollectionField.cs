using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TItem">The type of item that the collection stores.</typeparam>
/// <typeparam name="TItemField">The data type that represents the format by which <typeparamref name="TItem"/> is read and written.</typeparam>
public class DataStructureCollectionField<TStructure, TItem, TItemField> : BaseDataStructureFieldWithProperty<TStructure, ArrayField<TItemField>, List<TItem>?>
where TStructure : IDataStructure
where TItem : notnull
where TItemField : IBinaryField
{
	private readonly Func<TItemField>                 itemFactory;
	private readonly IFieldAdapter<TItemField, TItem> itemFieldAdapter;

	public DataStructureCollectionField(
		Func<TItemField> itemFactory,
		Expression<Func<TStructure, List<TItem>?>> propertyExpression,
		IFieldAdapter<TItemField, TItem> itemFieldAdapter
	) : base(() => new ArrayField<TItemField>(itemFactory), propertyExpression)
	{
		this.itemFactory = itemFactory;
		this.itemFieldAdapter = itemFieldAdapter;

		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureCollectionField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[array of {typeof(TItem).Name}]";

	protected override ArrayField<TItemField> GetFieldForStorage(TStructure structure)
	{
		var collection = GetSourceCollection(structure);

		if (collection is null)
			throw new InvalidOperationException($"Source collection was null for property \"{FriendlyDescription}\" while writing data");

		var field = CreateFieldInstance();

		field.Value.Clear();

		foreach (var item in collection)
		{
			var entry = this.itemFactory();

			this.itemFieldAdapter.Put(ref entry, item);
			field.Value.Add(entry);
		}

		return field;
	}

	protected override void LoadFieldFromStorage(TStructure structure, ArrayField<TItemField> data)
	{
		var items = data.Value.Select(this.itemFieldAdapter.Get);

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

	private List<TItem>? GetSourceCollection(TStructure structure)
		=> (List<TItem>?) PropertyInfo.GetValue(structure);
}

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TCollection">The type of data that the collection stores.</typeparam>
public class DataStructureCollectionField<TStructure, TCollection>(
	Func<TCollection> itemFactory,
	Expression<Func<TStructure, List<TCollection>?>> propertyExpression
) : DataStructureCollectionField<TStructure, TCollection, TCollection>(itemFactory, propertyExpression, PassthroughFieldAdapter<TCollection>.Instance)
where TStructure : IDataStructure
where TCollection : IBinaryField;