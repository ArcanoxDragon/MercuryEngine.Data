using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.Core.Extensions;

public static class ReflectionExtensions
{
	public static string GetDisplayName(this Type type)
	{
		var nonGenericTypeName = type.Name.Split('`')[0]; // Any type args suffix, e.g. "`2", is ignored

		if (type is { IsConstructedGenericType: true, GenericTypeArguments.Length: > 0 })
		{
			var typeArgumentNames = string.Join(", ", type.GenericTypeArguments.Select(t => t.GetDisplayName()));

			return $"{nonGenericTypeName}<{typeArgumentNames}>";
		}

		return nonGenericTypeName;
	}

	public static IEnumerable<Type> GetAllInterfaces(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		this Type type
	)
	{
		foreach (var @interface in type.GetInterfaces())
			yield return @interface;

		if (type.BaseType is { } baseType)
		{
			foreach (var baseInterface in baseType.GetAllInterfaces())
				yield return baseInterface;
		}
	}

	public static bool IsInstanceOfGeneric(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		this Type type,
		Type genericType
	)
	{
		if (!genericType.IsGenericType)
			throw new ArgumentException($"The \"{nameof(genericType)}\" parameter must be a generic type", nameof(genericType));

		if (CheckType(type))
			return true;

		var interfaces = type.GetAllInterfaces();

		foreach (var @interface in interfaces)
		{
			if (CheckType(@interface))
				return true;
		}

		if (type.BaseType is { } baseType && CheckType(baseType))
			return true;

		return false;

		bool CheckType(Type typeToCheck)
		{
			if (genericType == typeToCheck)
				return true;

			if (genericType.IsGenericTypeDefinition && typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericType)
				return true;

			if (genericType.IsConstructedGenericType && typeToCheck == genericType)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Returns whether or not this <see cref="Type"/> represents the same type as <paramref name="other"/>
	/// <i>or</i> the nullable version of <paramref name="other"/>, if <paramref name="other"/> is a value type.
	/// </summary>
	public static bool IsTypeOrNullable(this Type type, Type other)
	{
		if (type == other)
			return true;

		if (!type.IsConstructedGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
			return false;

		var nullableInnerType = Nullable.GetUnderlyingType(type);

		return nullableInnerType == other;
	}
}