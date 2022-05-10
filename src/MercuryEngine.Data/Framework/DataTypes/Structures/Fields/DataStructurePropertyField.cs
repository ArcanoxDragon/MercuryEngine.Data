using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Framework.DataAdapters;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing the value of a property on a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="TStructure">The type of the <see cref="DataStructure{T}"/> that contains the property to be read/written.</typeparam>
/// <typeparam name="TProperty">The type of value that the property stores.</typeparam>
/// <typeparam name="TData">The <see cref="IBinaryDataType"/> that represents the binary format used for reading and writing the property's value.</typeparam>
public class DataStructurePropertyField<TStructure, TProperty, TData> : BaseDataStructureField<TStructure, TData>
where TStructure : IDataStructure
where TProperty : notnull
where TData : IBinaryDataType
{
	private readonly PropertyInfo                   propertyInfo;
	private readonly IDataAdapter<TData, TProperty> dataAdapter;

	public DataStructurePropertyField(
		Func<TData> dataTypeFactory,
		Expression<Func<TStructure, TProperty>> propertyExpression,
		IDataAdapter<TData, TProperty> dataAdapter
	) : base(dataTypeFactory)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);
		this.dataAdapter = dataAdapter;

		if (!this.propertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructurePropertyField");
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[{typeof(TProperty).Name},{typeof(TData).Name}]";

	protected override TData GetData(TStructure structure)
	{
		var value = (TProperty?) this.propertyInfo.GetValue(structure);

		if (value is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

		var data = CreateDataType();

		this.dataAdapter.Put(data, value);

		return data;
	}

	protected override void PutData(TStructure structure, TData data)
	{
		if (!this.propertyInfo.CanWrite)
			return;

		var value = this.dataAdapter.Get(data);

		this.propertyInfo.SetValue(structure, value);
	}
}