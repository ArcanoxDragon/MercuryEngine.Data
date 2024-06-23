using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Test.Utility;

public class TerminatedStringFieldJsonConverter : JsonConverter<TerminatedStringField>
{
	public override bool HandleNull => true;

	public override void Write(Utf8JsonWriter writer, TerminatedStringField? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.Value);
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, TerminatedStringField value, JsonSerializerOptions options)
	{
		if (string.IsNullOrEmpty(value?.Value))
			throw new ArgumentException($"Attempted to write a null or empty {nameof(TerminatedStringField)} as a property name");

		writer.WritePropertyName(value.Value);
	}

	public override TerminatedStringField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();
}