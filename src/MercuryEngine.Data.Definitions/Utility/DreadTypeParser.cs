using System.Text;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Definitions.Extensions;
using MercuryEngine.Data.Definitions.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MercuryEngine.Data.Definitions.Utility;

internal static class DreadTypeParser
{
	private static readonly JsonSerializerSettings JsonSettings = new() {
		ContractResolver = new DefaultContractResolver {
			NamingStrategy = new SnakeCaseNamingStrategy(),
		},
		Converters = {
			new DreadTypeConverter(),
		},
	};

	public static Dictionary<string, BaseDreadType> ParseDreadTypes()
	{
		using var fileStream = ResourceHelper.OpenResourceFile("DataDefinitions/dread_types.json");
		using var reader = new StreamReader(fileStream, Encoding.UTF8);
		var jsonText = reader.ReadToEnd();
		var typesDictionary = JsonConvert.DeserializeObject<Dictionary<string, BaseDreadType>>(jsonText, JsonSettings)
							  ?? throw new InvalidOperationException("Unable to read the type definition database!");

		foreach (var (typeName, type) in typesDictionary)
			type.TypeName = typeName;

		return typesDictionary;
	}
}