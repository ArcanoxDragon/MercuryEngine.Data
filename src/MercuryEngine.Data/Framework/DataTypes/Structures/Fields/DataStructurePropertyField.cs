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
where TStructure : DataStructure<TStructure>
where TProperty : notnull
where TData : IBinaryDataType, new()
{
	private readonly PropertyInfo                   propertyInfo;
	private readonly IDataAdapter<TProperty, TData> dataAdapter;

	public DataStructurePropertyField(
		TStructure structure,
		Expression<Func<TStructure, TProperty>> propertyExpression,
		IDataAdapter<TProperty, TData> dataAdapter
	) : base(structure)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);
		this.dataAdapter = dataAdapter;

		if (!this.propertyInfo.CanRead || !this.propertyInfo.CanWrite)
			throw new ArgumentException("A property must have both a getter and a setter in order to be used in a DataStructurePropertyField");
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[{typeof(TProperty).Name},{typeof(TData).Name}]";

	protected override TData Data { get; } = new();

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		// Synchronize value of Data to property
		this.propertyInfo.SetValue(Structure, this.dataAdapter.Get(Data));
	}

	public override void Write(BinaryWriter writer)
	{
		// Synchronize value of property to Data
		var value = (TProperty?) this.propertyInfo.GetValue(Structure);

		if (value is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{this.propertyInfo.Name}\" was null.");

		this.dataAdapter.Put(Data, value);

		base.Write(writer);
	}
}