using System.Text.Json.Serialization;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Definitions.Json;

[JsonSerializable(typeof(DreadPrimitiveType))]
[JsonSerializable(typeof(DreadStructType))]
[JsonSerializable(typeof(DreadEnumType))]
[JsonSerializable(typeof(DreadTypedefType))]
[JsonSerializable(typeof(DreadVectorType))]
[JsonSerializable(typeof(DreadDictionaryType))]
[JsonSerializable(typeof(DreadPointerType))]
[JsonSerializable(typeof(DreadFlagsetType))]
[JsonSerializable(typeof(Dictionary<string, BaseDreadType>))]
[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	Converters = [
		typeof(DreadTypeConverter),
		typeof(JsonStringEnumConverter<DreadPrimitiveKind>),
		typeof(JsonStringEnumConverter<DreadTypeKind>),
	]
)]
internal partial class DreadTypesJsonContext : JsonSerializerContext;