using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.Tests.Extensions;

internal static class LinqExtensions
{
	[return: NotNullIfNotNull(nameof(defaultValue))]
	public static T? MaxOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default)
	where T : IComparable<T>
	{
		var maximum = default(T);
		var foundAny = false;

		foreach (var item in source)
		{
			if (!foundAny || item.CompareTo(maximum) > 0)
				maximum = item;

			foundAny = true;
		}

		return foundAny ? maximum : defaultValue;
	}
}