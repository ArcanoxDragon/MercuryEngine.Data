using MercuryEngine.Data.Definitions.DreadTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MercuryEngine.Data.Definitions.Json;

public class DreadTypeConverter : JsonConverter<BaseDreadType>
{
	private static readonly JsonSerializerSettings InnerSettings = new() {
		ContractResolver = new DefaultContractResolver {
			NamingStrategy = new SnakeCaseNamingStrategy(),
		},
	};

	private static readonly JsonSerializer InnerSerializer = JsonSerializer.CreateDefault(InnerSettings);

	public override void WriteJson(JsonWriter writer, BaseDreadType? value, JsonSerializer serializer)
	{
		if (value is null)
		{
			writer.WriteNull();
			return;
		}

		serializer.Serialize(writer, value, value.GetType());
	}

	public override BaseDreadType? ReadJson(JsonReader reader, Type objectType, BaseDreadType? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue is not null)
			return existingValue;

		if (reader.TokenType is JsonToken.Null)
			return null;

		var obj = JObject.Load(reader);

		if (!obj.TryGetValue("kind", out var kindValue))
			throw new JsonException("Object is missing the \"kind\" property");

		if (kindValue.Type is not JTokenType.String)
			throw new JsonException("The \"kind\" property was not a string");

		var kind = (string) kindValue!;

		if (!Enum.TryParse(kind, true, out DreadTypeKind typeKind))
			throw new JsonException($"Unrecognized \"kind\" value \"{kind}\"");

		var concreteType = MapTypeKind(typeKind);

		return (BaseDreadType?) obj.ToObject(concreteType, InnerSerializer);
	}

	private static Type MapTypeKind(DreadTypeKind typeKind) => typeKind switch {
		DreadTypeKind.Primitive  => typeof(DreadPrimitiveType),
		DreadTypeKind.Struct     => typeof(DreadStructType),
		DreadTypeKind.Enum       => typeof(DreadEnumType),
		DreadTypeKind.Typedef    => typeof(DreadTypedefType),
		DreadTypeKind.Vector     => typeof(DreadVectorType),
		DreadTypeKind.Dictionary => typeof(DreadDictionaryType),
		DreadTypeKind.Pointer    => typeof(DreadPointerType),
		DreadTypeKind.Flagset    => typeof(DreadFlagsetType),

		_ => throw new InvalidOperationException($"Unknown or unsupported type kind \"{typeKind}\""),
	};
}