using System.Text.Json;
using System.Text.RegularExpressions;

namespace MercuryEngine.Data.Definitions.Json;

public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
	private static readonly Regex CamelCaseRegex = new(@"([a-z])([A-Z])", RegexOptions.Compiled);

	public override string ConvertName(string name)
		=> CamelCaseRegex.Replace(name, match => $"{match.Groups[1].Value}_{match.Groups[2].Value}").ToLower();
}