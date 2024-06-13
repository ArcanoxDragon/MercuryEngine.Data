using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Definitions.DreadTypes;

namespace MercuryEngine.Data.Definitions.Json;

public class DreadTypeConverter : JsonConverter<BaseDreadType>
{
	private static readonly JsonSerializerOptions InnerOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
	};

	public override BaseDreadType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected {JsonTokenType.StartObject} but found {reader.TokenType} while reading Dread data type");

		if (JsonNode.Parse(ref reader) is not JsonObject obj)
			return null;

		if (obj["kind"]?.GetValue<string>() is not { } kind)
			throw new JsonException($"Object is missing the \"kind\" property, or the property was not a string");

		if (!Enum.TryParse(kind, true, out DreadTypeKind typeKind))
			throw new JsonException($"Unrecognized \"kind\" value \"{kind}\"");

		var concreteType = MapTypeKind(typeKind);

		return (BaseDreadType?) obj.Deserialize(concreteType, options);
	}

	public override void Write(Utf8JsonWriter writer, BaseDreadType value, JsonSerializerOptions options)
	{
		if (value.GetType() == typeof(BaseDreadType))
			// This should NEVER happen, but if it SOMEHOW does, we want to throw instead of infinitely recursing
			throw new JsonException($"Tried to serialize non-inherited instance of {nameof(BaseDreadType)}!");

		JsonSerializer.Serialize(writer, value, value.GetType(), options);
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