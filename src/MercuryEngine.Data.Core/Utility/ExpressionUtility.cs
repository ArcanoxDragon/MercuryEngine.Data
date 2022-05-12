using System.Linq.Expressions;
using System.Reflection;

namespace MercuryEngine.Data.Core.Utility;

public static class ExpressionUtility
{
	public static PropertyInfo GetProperty(LambdaExpression expression)
	{
		if (expression is not { Body: MemberExpression { Member: PropertyInfo propertyInfo } })
			throw new ArgumentException("Expression must be a simple property access expression", nameof(expression));

		return propertyInfo;
	}
}