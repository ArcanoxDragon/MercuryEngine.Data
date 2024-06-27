using System.Text;
using System.Text.Json;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Json;

#if !NET8_0_OR_GREATER
using MercuryEngine.Data.Definitions.Extensions;
#endif

namespace MercuryEngine.Data.Definitions.Utility;

internal static class DreadTypeParser
{
	private static readonly JsonSerializerOptions JsonOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		Converters = {
			new DreadTypeConverter(),
		},
	};

	public static Dictionary<string, BaseDreadType> ParseDreadTypes()
	{
		using var fileStream = ResourceHelper.OpenResourceFile("DataDefinitions/dread_types.json");
		using var reader = new StreamReader(fileStream, Encoding.UTF8);
		var jsonText = reader.ReadToEnd();
		var typesDictionary = JsonSerializer.Deserialize<Dictionary<string, BaseDreadType>>(jsonText, JsonOptions)
							  ?? throw new InvalidOperationException("Unable to read the type definition database!");

		foreach (var (typeName, type) in typesDictionary)
			type.TypeName = typeName;

		return typesDictionary;
	}
}