using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class EnumFieldJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsInstanceOfGeneric(typeof(EnumField<>));

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (!typeToConvert.IsConstructedGenericType)
			return null;

		var enumValueType = typeToConvert.GetGenericArguments()[0];
		var enumFieldType = typeof(EnumField<>).MakeGenericType(enumValueType);

		if (!typeToConvert.IsAssignableTo(enumFieldType))
			return null;

		var converterType = typeof(EnumFieldJsonConverter<>).MakeGenericType(enumValueType);

		return (JsonConverter?) Activator.CreateInstance(converterType);
	}
}

internal class EnumFieldJsonConverter<T> : JsonConverter<EnumField<T>>
where T : struct, Enum
{
	public override bool HandleNull => true;

	public override void Write(Utf8JsonWriter writer, EnumField<T>? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.Value.ToString());
	}

	public override void WriteAsPropertyName(Utf8JsonWriter writer, EnumField<T> value, JsonSerializerOptions options)
	{
		if (string.IsNullOrEmpty(value?.Value.ToString()))
			throw new ArgumentException($"Attempted to write a null or empty {nameof(EnumField<T>)} as a property name");

		writer.WritePropertyName(value.Value.ToString());
	}

	public override EnumField<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();
}