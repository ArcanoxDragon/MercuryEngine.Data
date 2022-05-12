using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.DataAdapters;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TProperty">The type of value that the property stores.</typeparam>
/// <typeparam name="TData">The <see cref="IBinaryDataType"/> that represents the binary format used for reading and writing the property's value.</typeparam>
public class DataStructurePropertyField<TStructure, TProperty, TData> : BaseDataStructureFieldWithProperty<TStructure, TData, TProperty>
where TStructure : IDataStructure
where TData : IBinaryDataType
{
	private readonly IDataAdapter<TData, TProperty> dataAdapter;

	public DataStructurePropertyField(
		Func<TData> dataTypeFactory,
		Expression<Func<TStructure, TProperty?>> propertyExpression,
		IDataAdapter<TData, TProperty> dataAdapter
	) : base(dataTypeFactory, propertyExpression)
	{
		this.dataAdapter = dataAdapter;

		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructurePropertyField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[{typeof(TProperty).Name},{typeof(TData).Name}]";

	protected override TData GetData(TStructure structure)
	{
		var value = (TProperty?) PropertyInfo.GetValue(structure);

		if (value is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{PropertyInfo.Name}\" was null while writing data.");

		var data = CreateDataType();

		this.dataAdapter.Put(ref data, value);

		return data;
	}

	protected override void PutData(TStructure structure, TData data)
	{
		if (!PropertyInfo.CanWrite)
			return;

		var value = this.dataAdapter.Get(data);

		PropertyInfo.SetValue(structure, value);
	}
}