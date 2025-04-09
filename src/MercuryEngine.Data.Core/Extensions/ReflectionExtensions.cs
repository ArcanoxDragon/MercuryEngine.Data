using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.Core.Extensions;

internal static class ReflectionExtensions
{
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
}