using System.Text.RegularExpressions;

namespace MercuryEngine.Data.Core.Extensions;

public static class RegexExtensions
{
	/// <summary>
	/// Evalutes this <see cref="Regex"/> against the provided <paramref name="input"/> string, and returns
	/// whether or not the resulting <see cref="Match"/> was successful.
	/// </summary>
	public static bool IsMatch(this Regex regex, string input, int startAt, out Match match)
	{
		match = regex.Match(input, startAt);
		return match.Success;
	}

	/// <summary>
	/// Evalutes this <see cref="Regex"/> against the provided <paramref name="input"/> string, and returns
	/// whether or not the resulting <see cref="Match"/> was successful.
	/// </summary>
	public static bool IsMatch(this Regex regex, string input, out Match match)
	{
		match = regex.Match(input);
		return match.Success;
	}
}