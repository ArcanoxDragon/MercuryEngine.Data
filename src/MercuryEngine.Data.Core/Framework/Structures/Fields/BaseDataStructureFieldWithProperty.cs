using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Structures.Fields;

public abstract class BaseDataStructureFieldWithProperty<TStructure, TData, TProperty> : BaseDataStructureField<TStructure, TData>
where TStructure : IDataStructure
where TData : IBinaryDataType
{
	private static readonly NullabilityInfoContext NullabilityInfoContext = new();

	protected BaseDataStructureFieldWithProperty(Func<TData> dataTypeFactory, Expression<Func<TStructure, TProperty?>> propertyExpression) : base(dataTypeFactory)
	{
		PropertyInfo = ExpressionUtility.GetProperty(propertyExpression);
		NullabilityInfo = NullabilityInfoContext.Create(PropertyInfo);
	}

	public override string FriendlyDescription => PropertyInfo.Name;

	protected PropertyInfo    PropertyInfo    { get; }
	protected NullabilityInfo NullabilityInfo { get; }

	public override void ClearData(TStructure structure)
	{
		if (NullabilityInfo.WriteState is NullabilityState.NotNull)
			// Don't clear out properties that are explicitly marked not-null
			return;

		PropertyInfo.SetValue(structure, null);
	}

	public override bool HasData(TStructure structure)
		=> PropertyInfo.GetValue(structure) is not null;
}