using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Test.Utility;

public class NumberFieldJsonConverter<TBinaryNumber, TNumber> : JsonConverter<TBinaryNumber>
where TBinaryNumber : NumberField<TNumber>
where TNumber : unmanaged
{
	public override bool HandleNull => true;

	public override void Write(Utf8JsonWriter writer, TBinaryNumber? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteRawValue(value.Value.ToString()!.ToLower());
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, TBinaryNumber? value, JsonSerializerOptions options)
	{
		if (value is null)
			throw new ArgumentException($"Attempted to write a null {nameof(TBinaryNumber)} as a property name");

		writer.WritePropertyName(value.Value.ToString()!);
	}

	public override TBinaryNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();
}