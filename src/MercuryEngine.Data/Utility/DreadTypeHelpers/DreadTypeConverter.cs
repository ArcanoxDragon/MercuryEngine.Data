using System.Text.Json;
using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

public class DreadTypeConverter : JsonConverter<BaseDreadType>
{
	public override BaseDreadType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is JsonTokenType.Null)
			return null;

		var startToken = reader;
		using var currentDocument = JsonDocument.ParseValue(ref reader);
		var currentObject = currentDocument.RootElement;

		if (!currentObject.TryGetProperty("kind", out var kindElement))
			throw new JsonException("Object is missing the \"kind\" property");

		if (kindElement.GetString() is not { } kind)
			throw new JsonException("The \"kind\" property was not a string");

		if (!Enum.TryParse(kind, true, out DreadTypeKind typeKind))
			throw new JsonException($"Unrecognized \"kind\" value \"{kind}\"");

		var concreteType = MapTypeKind(typeKind);

		return (BaseDreadType?) JsonSerializer.Deserialize(ref startToken, concreteType, options);
	}

	public override void Write(Utf8JsonWriter writer, BaseDreadType value, JsonSerializerOptions options)
		=> JsonSerializer.Serialize(writer, value, value.GetType(), options);

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