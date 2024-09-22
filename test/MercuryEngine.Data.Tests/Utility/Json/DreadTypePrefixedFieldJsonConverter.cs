using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class DreadTypePrefixedFieldJsonConverter : JsonConverter<DreadTypePrefixedField>
{
	public override bool HandleNull => true;

	public override DreadTypePrefixedField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	public override void Write(Utf8JsonWriter writer, DreadTypePrefixedField value, JsonSerializerOptions options)
	{
		if (value.InnerData is null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value.InnerData, value.InnerData.GetType(), options);
	}
}