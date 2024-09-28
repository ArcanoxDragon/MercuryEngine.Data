using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class DreadPointerJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsInstanceOfGeneric(typeof(DreadPointer<>));

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (!typeToConvert.IsConstructedGenericType)
			return null;

		var pointerValueType = typeToConvert.GetGenericArguments()[0];
		var pointerFieldType = typeof(DreadPointer<>).MakeGenericType(pointerValueType);

		if (!typeToConvert.IsAssignableTo(pointerFieldType))
			return null;

		var converterType = typeof(DreadPointerJsonConverter<>).MakeGenericType(pointerValueType);

		return (JsonConverter?) Activator.CreateInstance(converterType);
	}
}

internal class DreadPointerJsonConverter<T> : JsonConverter<DreadPointer<T>>
where T : class, ITypedDreadField
{
	public override bool HandleNull => true;

	public override DreadPointer<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	public override void Write(Utf8JsonWriter writer, DreadPointer<T> value, JsonSerializerOptions options)
	{
		if (value.Value is null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);
	}
}