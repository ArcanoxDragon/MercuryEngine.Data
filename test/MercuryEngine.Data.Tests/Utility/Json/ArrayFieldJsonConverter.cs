using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class ArrayFieldJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsInstanceOfGeneric(typeof(ArrayField<>));

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (!typeToConvert.IsConstructedGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(ArrayField<>))
			return null;

		var arrayItemType = typeToConvert.GetGenericArguments()[0];
		var converterType = typeof(ArrayFieldJsonConverter<>).MakeGenericType(arrayItemType);

		return (JsonConverter?) Activator.CreateInstance(converterType);
	}
}

internal class ArrayFieldJsonConverter<TItem> : JsonConverter<ArrayField<TItem>>
where TItem : IBinaryField
{
	public override ArrayField<TItem> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	public override void Write(Utf8JsonWriter writer, ArrayField<TItem> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();

		foreach (var item in value.Value)
			JsonSerializer.Serialize(writer, item, item.GetType(), options);

		writer.WriteEndArray();
	}
}