using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MercuryEngine.Data.Core.Utility;

internal static class ReflectionUtility
{
	public static Func<T> CreateFactoryFromDefaultConstructor<T>([CallerMemberName] string? callerMemberName = null)
	{
		var defaultConstructor = typeof(T).GetConstructor([]);

		if (defaultConstructor is null)
			throw new MissingMethodException($"The type \"{typeof(T).FullName}\" must have a public parameterless constructor in order to be used in {callerMemberName}");

		var newExpression = Expression.New(defaultConstructor); // new T()
		var factoryLambda = Expression.Lambda<Func<T>>(newExpression);

		return factoryLambda.Compile();
	}

	public static PropertyInfo GetProperty(LambdaExpression expression)
	{
		if (expression is not { Body: MemberExpression { Member: PropertyInfo propertyInfo } })
			throw new ArgumentException("Expression must be a simple property access expression", nameof(expression));

		return propertyInfo;
	}
}