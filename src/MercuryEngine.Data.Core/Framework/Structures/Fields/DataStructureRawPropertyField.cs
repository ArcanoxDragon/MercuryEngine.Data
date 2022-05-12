using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.DataTypes;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing a raw <see cref="IBinaryDataType"/> that
/// is a child of (e.g. property on) a <see cref="DataStructure{T}"/>.
/// </summary>
public class DataStructureRawPropertyField<TStructure, TData> : BaseDataStructureFieldWithProperty<TStructure, TData, TData?>
where TStructure : IDataStructure
where TData : class, IBinaryDataType
{
	public DataStructureRawPropertyField(
		Func<TData> dataTypeFactory,
		Expression<Func<TStructure, TData?>> propertyExpression
	) : base(dataTypeFactory, propertyExpression)
	{
		if (!PropertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureRawPropertyField");
	}

	public override string FriendlyDescription => $"{PropertyInfo.Name}[{typeof(TData).Name}]";

	protected override TData GetData(TStructure structure)
	{
		var data = (TData?) PropertyInfo.GetValue(structure);

		if (data is null)
			throw new InvalidOperationException($"The value retrieved from {typeof(TStructure).Name} property \"{PropertyInfo.Name}\" was null.");

		return data;
	}

	protected override void PutData(TStructure structure, TData data)
	{
		if (!PropertyInfo.CanWrite)
			return;

		PropertyInfo.SetValue(structure, data);
	}
}