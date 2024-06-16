using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.DataAdapters;

public class PropertyExpressionAdapter<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> propertyExpression) : IDataAdapter<TOwner, TProperty>
{
	private readonly PropertyInfo propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

	public string PropertyName => this.propertyInfo.Name;

	public TProperty Get(TOwner storage)
	{
		if (!this.propertyInfo.CanRead)
			throw new InvalidOperationException($"Property \"{this.propertyInfo.Name}\" on type \"{typeof(TOwner).FullName}\" does not have a getter");

		return (TProperty) this.propertyInfo.GetValue(storage)!;
	}

	public void Put(ref TOwner storage, TProperty value)
	{
		if (!this.propertyInfo.CanWrite)
			throw new InvalidOperationException($"Property \"{this.propertyInfo.Name}\" on type \"{typeof(TOwner).FullName}\" does not have a setter");

		this.propertyInfo.SetValue(storage, value);
	}
}