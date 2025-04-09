using System.Text.Json;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Mapping;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal sealed class DataMapperJsonConverter : JsonConverter<DataMapper>
{
	public override DataMapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException("Reading is not supported");

	public override void Write(Utf8JsonWriter writer, DataMapper value, JsonSerializerOptions options)
		=> JsonSerializer.Serialize(writer, value.RootRange, options);
}