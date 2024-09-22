using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class DreadTypedFieldJsonConverter : JsonConverter<ITypedDreadField>
{
	public override ITypedDreadField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	public override void Write(Utf8JsonWriter writer, ITypedDreadField value, JsonSerializerOptions options)
		=> JsonSerializer.Serialize(writer, value, value.GetType(), options);
}