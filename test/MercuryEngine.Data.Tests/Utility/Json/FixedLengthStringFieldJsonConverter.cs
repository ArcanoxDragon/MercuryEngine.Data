using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

public class FixedLengthStringFieldJsonConverter : JsonConverter<FixedLengthStringField>
{
	public override bool HandleNull => true;

	public override void Write(Utf8JsonWriter writer, FixedLengthStringField? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.Value);
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, FixedLengthStringField value, JsonSerializerOptions options)
	{
		if (string.IsNullOrEmpty(value?.Value))
			throw new ArgumentException($"Attempted to write a null or empty {nameof(FixedLengthStringField)} as a property name");

		writer.WritePropertyName(value.Value);
	}

	public override FixedLengthStringField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();
}