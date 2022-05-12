using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

public class PropertyExpressionAdapter<TOwner, TProperty> : IDataAdapter<TOwner, TProperty>
{
	private readonly PropertyInfo propertyInfo;

	public PropertyExpressionAdapter(Expression<Func<TOwner, TProperty>> propertyExpression)
	{
		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);
	}

	public string PropertyName => this.propertyInfo.Name;

	public TProperty Get(TOwner storage)
	{
		if (!this.propertyInfo.CanRead)
			throw new InvalidOperationException($"Property \"{this.propertyInfo.Name}\" on type \"{typeof(TOwner).FullName}\" does not have a getter");

		return (TProperty) this.propertyInfo.GetValue(storage)!;
	}

	public void Put(TOwner storage, TProperty value)
	{
		if (!this.propertyInfo.CanWrite)
			throw new InvalidOperationException($"Property \"{this.propertyInfo.Name}\" on type \"{typeof(TOwner).FullName}\" does not have a setter");

		this.propertyInfo.SetValue(storage, value);
	}
}