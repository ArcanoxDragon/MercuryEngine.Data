using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Framework.DataTypes.Structures.Fields;

/// <summary>
/// An <see cref="IDataStructureField"/> that handles reading and writing a raw <see cref="IBinaryDataType"/> that
/// is a child of (e.g. property on) a <see cref="DataStructure{T}"/>.
/// </summary>
public class DataStructureRawPropertyField<TStructure, TData> : BaseDataStructureField<TStructure, TData>
where TStructure : DataStructure<TStructure>
where TData : class, IBinaryDataType
{
	private readonly PropertyInfo propertyInfo;

	public DataStructureRawPropertyField(
		TStructure structure,
		Expression<Func<TStructure, TData>> propertyExpression
	) : base(structure)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

		if (!this.propertyInfo.CanRead)
			throw new ArgumentException("A property must have a getter in order to be used in a DataStructureRawPropertyField");
	}

	public override string FriendlyDescription => $"{this.propertyInfo.Name}[{typeof(TData).Name}]";

	protected override TData Data => (TData) this.propertyInfo.GetValue(Structure)!;
}