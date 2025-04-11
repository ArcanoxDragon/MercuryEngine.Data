using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class DictionaryFieldJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsInstanceOfGeneric(typeof(DictionaryField<,>));

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (!typeToConvert.IsConstructedGenericType || !typeToConvert.IsInstanceOfGeneric(typeof(DictionaryField<,>)))
			return null;

		var genericArgs = typeToConvert.GetGenericArguments();
		var converterType = typeof(DictionaryFieldJsonConverter<,>).MakeGenericType(genericArgs);

		return (JsonConverter?) Activator.CreateInstance(converterType);
	}
}

internal class DictionaryFieldJsonConverter<TKey, TValue> : JsonConverter<DictionaryField<TKey, TValue>>
where TKey : IBinaryField
where TValue : IBinaryField
{
	public override DictionaryField<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	public override void Write(Utf8JsonWriter writer, DictionaryField<TKey, TValue> field, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		foreach (var (key, value) in field.Value)
		{
			string keyString = JsonSerializer.Serialize(key, key.GetType(), options);

			writer.WritePropertyName(keyString);
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}

		writer.WriteStartObject();
	}
}