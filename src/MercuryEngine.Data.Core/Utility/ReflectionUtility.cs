using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Utility;

internal static class ReflectionUtility
{
	public static NullabilityInfoContext NullabilityInfoContext { get; } = new();

	public static Func<T> CreateFactoryFromDefaultConstructor<T>([CallerMemberName] string? callerMemberName = null)
	{
		var defaultConstructor = typeof(T).GetConstructor([]);

		if (defaultConstructor is null)
			throw new MissingMethodException($"The type \"{typeof(T).FullName}\" must have a public parameterless constructor in order to be used in {callerMemberName}");

		var newExpression = Expression.New(defaultConstructor); // new T()
		var factoryLambda = Expression.Lambda<Func<T>>(newExpression, tailCall: true);

		return factoryLambda.Compile();
	}

	public static PropertyInfo GetProperty(LambdaExpression expression)
	{
		if (expression is not { Body: MemberExpression { Member: PropertyInfo propertyInfo } })
			throw new ArgumentException("Expression must be a simple property access expression", nameof(expression));

		return propertyInfo;
	}

	#region Reflection JIT

	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	private record struct MemberCacheKey(Type ValueType, Type OwnerType, Type PropertyType, string MemberName)
	{
		public MemberCacheKey(Type valueType, PropertyInfo propertyInfo)
			: this(valueType, propertyInfo.DeclaringType!, propertyInfo.PropertyType, propertyInfo.Name) { }
	}

	private sealed class MemberCacheKeyEqualityComparer : IEqualityComparer<MemberCacheKey>
	{
		public static MemberCacheKeyEqualityComparer Instance { get; } = new();

		public bool Equals(MemberCacheKey x, MemberCacheKey y)
			=> x.ValueType == y.ValueType &&
			   x.OwnerType == y.OwnerType &&
			   x.PropertyType == y.PropertyType &&
			   x.MemberName == y.MemberName;

		public int GetHashCode(MemberCacheKey obj)
			=> HashCode.Combine(obj.ValueType, obj.OwnerType, obj.PropertyType, obj.MemberName);
	}

	private static readonly ConcurrentDictionary<MemberCacheKey, Delegate> PropertyGetterCache = new(MemberCacheKeyEqualityComparer.Instance);
	private static readonly ConcurrentDictionary<MemberCacheKey, Delegate> PropertySetterCache = new(MemberCacheKeyEqualityComparer.Instance);

	public static Func<object, T> GetGetter<T>(PropertyInfo property)
	{
		var key = new MemberCacheKey(typeof(T), property);

		return (Func<object, T>) PropertyGetterCache.GetOrAdd(key, _ => CompileGetter<T>(property));
	}

	public static Action<object, T> GetSetter<T>(PropertyInfo property)
	{
		var key = new MemberCacheKey(typeof(T), property);

		return (Action<object, T>) PropertySetterCache.GetOrAdd(key, _ => CompileSetter<T>(property));
	}

	private static Func<object, T> CompileGetter<T>(PropertyInfo property)
	{
		if (!property.PropertyType.IsAssignableTo(typeof(T)))
			throw new ArgumentException($"{nameof(CompileGetter)} was called with type \"{typeof(T).FullName}\", which is not " +
										$"compatible with the provided property's type \"{property.PropertyType.FullName}\"");

		var ownerParameter = Expression.Parameter(typeof(object), "owner");
		var ownerCastExpression = Expression.Convert(ownerParameter, property.DeclaringType!);
		var propertyExpression = Expression.MakeMemberAccess(ownerCastExpression, property);
		Expression bodyExpression = propertyExpression;

		if (property.PropertyType != typeof(T))
			bodyExpression = Expression.Convert(bodyExpression, typeof(T));

		return Expression.Lambda<Func<object, T>>(bodyExpression, tailCall: true, ownerParameter).Compile();
	}

	private static Action<object, T> CompileSetter<T>(PropertyInfo property)
	{
		if (!typeof(T).IsAssignableTo(property.PropertyType))
			throw new ArgumentException($"{nameof(CompileSetter)} was called with type \"{typeof(T).FullName}\", which is " +
										$"not assignable to the provided property's type \"{property.PropertyType.FullName}\"");

		var ownerParameter = Expression.Parameter(typeof(object), "owner");
		var valueParameter = Expression.Parameter(typeof(T), "value");
		var ownerCastExpression = Expression.Convert(ownerParameter, property.DeclaringType!);
		var propertyExpression = Expression.MakeMemberAccess(ownerCastExpression, property);
		Expression assignmentRightSideExpression = valueParameter;

		if (property.PropertyType != typeof(T))
			assignmentRightSideExpression = Expression.Convert(assignmentRightSideExpression, property.PropertyType);

		var assignExpression = Expression.Assign(propertyExpression, assignmentRightSideExpression);

		return Expression.Lambda<Action<object, T>>(assignExpression, ownerParameter, valueParameter).Compile();
	}

	#endregion
}