using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TProperty">The type of value that the property stores.</typeparam>
/// <typeparam name="TField">The <see cref="IBinaryField"/> that represents the binary format used for reading and writing the property's value.</typeparam>
public class DataStructurePropertyField<TStructure, TProperty, TField> : BaseDataStructureFieldWithProperty<TStructure, TField, TProperty>
where TStructure : IDataStructure
where TField : IBinaryField
{
	private readonly IFieldAdapter<TField, TProperty> fieldAdapter;

	public DataStructurePropertyField(
		Func<TField> fieldFactory,
		Expression<Func<TStructure, TProperty?>> propertyExpression,
		IFieldAdapter<TField, TProperty> fieldAdapter
	) : base(fieldFactory, propertyExpression)
	{
		this.fieldAdapter = fieldAdapter;

		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructurePropertyField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[{typeof(TProperty).Name},{typeof(TField).Name}]";

	protected override TField GetFieldForStorage(TStructure structure)
	{
		var value = (TProperty?) PropertyInfo.GetValue(structure);

		if (value is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{PropertyInfo.Name}\" was null while writing data.");

		var field = CreateFieldInstance();

		this.fieldAdapter.Put(ref field, value);

		return field;
	}

	protected override void LoadFieldFromStorage(TStructure structure, TField data)
	{
		if (!PropertyInfo.CanWrite)
			return;

		var value = this.fieldAdapter.Get(data);

		PropertyInfo.SetValue(structure, value);
	}
}