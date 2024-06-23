using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Test.Utility;

internal class DreadTypePrefixedFieldJsonConverter : JsonConverter<DreadTypePrefixedField>
{
	public override bool HandleNull => true;

	public override DreadTypePrefixedField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotSupportedException();
		/*if (reader.TokenType == JsonTokenType.Null)
			return new DreadTypePrefixedField();

		var value = JsonSerializer.Deserialize(ref reader, typeof(DataWithDreadType), options);

		if (value is not DataWithDreadType data)
			return new DreadTypePrefixedField();

		return new DreadTypePrefixedField(data);*/
	}

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