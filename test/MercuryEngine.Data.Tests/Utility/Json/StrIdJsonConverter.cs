using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

public class StrIdJsonConverter : JsonConverter<StrId>
{
	public override void Write(Utf8JsonWriter writer, StrId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, StrId value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(value.ToString());
	}

	public override StrId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();
}