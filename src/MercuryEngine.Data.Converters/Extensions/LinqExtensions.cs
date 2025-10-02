using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.Converters.Extensions;

internal static class LinqExtensions
{
	public delegate bool TrySelectDelegate<in TSource, TResult>(TSource source, [NotNullWhen(true)] out TResult? result);

	public static IEnumerable<TResult> TrySelect<TSource, TResult>(this IEnumerable<TSource> source, TrySelectDelegate<TSource, TResult> @delegate)
	{
		foreach (var item in source)
		{
			if (@delegate(item, out var result))
				yield return result;
		}
	}
}