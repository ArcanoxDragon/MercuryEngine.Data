using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a collection (<see cref="List{T}"/>) property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TCollection">The type of item that the collection stores.</typeparam>
public class DataStructureCollectionField<TStructure, TCollection> : BaseDataStructureField<TStructure, ArrayDataType<TCollection>>
where TStructure : DataStructure<TStructure>
where TCollection : IBinaryDataType
{
	private readonly PropertyInfo propertyInfo;

	public DataStructureCollectionField(
		TStructure structure,
		Expression<Func<TStructure, List<TCollection>>> propertyExpression,
		Func<TCollection> entryFactory
	) : base(structure)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

		if (!this.propertyInfo.CanRead || this.propertyInfo.CanWrite)
			throw new ArgumentException("A property must be read-only in order to be used in a DataStructureCollectionField");

		if (!typeof(List<TCollection>).IsAssignableFrom(this.propertyInfo.PropertyType))
			throw new ArgumentException($"A property must be of type {nameof(List<TCollection>)} in order to be used in a DataStructureCollectionField");

		Data = new ArrayDataType<TCollection>(entryFactory);
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[array of {typeof(TCollection).Name}]";

	protected override ArrayDataType<TCollection> Data { get; }

	protected List<TCollection> SourceCollection
	{
		get
		{
			var propertyValue = this.propertyInfo.GetValue(Structure);

			if (propertyValue is null)
				throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

			if (propertyValue is not List<TCollection> collection)
				throw new InvalidOperationException();

			return collection;
		}
	}

	public override void Read(BinaryReader reader)
	{
		// Read into the source collection directly
		Data.Value = SourceCollection;

		base.Read(reader);
	}

	public override void Write(BinaryWriter writer)
	{
		// Ensure collections are synchronized
		Data.Value = SourceCollection;

		base.Write(writer);
	}
}