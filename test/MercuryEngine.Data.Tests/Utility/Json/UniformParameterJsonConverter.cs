using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Types.Bsmat;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class UniformParameterJsonConverter : JsonConverter<UniformParameter>
{
	public override void Write(Utf8JsonWriter writer, UniformParameter value, JsonSerializerOptions options)
	{
		var jsonObject = new JsonObject {
			["Name"] = value.Name,
			["Type"] = value.Type switch {
				UniformParameter.TypeFloat       => "float",
				UniformParameter.TypeSignedInt   => "int32",
				UniformParameter.TypeUnsignedInt => "uint32",
				_                                => "Unknown",
			},
			["Values"] = value.Type switch {
				UniformParameter.TypeFloat       => new JsonArray(value.FloatValues.Select(v => (JsonNode) v).ToArray()),
				UniformParameter.TypeSignedInt   => new JsonArray(value.SignedIntValues.Select(v => (JsonNode) v).ToArray()),
				UniformParameter.TypeUnsignedInt => new JsonArray(value.UnsignedIntValues.Select(v => (JsonNode) v).ToArray()),
				_                                => new JsonArray(),
			},
		};

		jsonObject.WriteTo(writer, options);
	}

	public override UniformParameter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException("Reading is not supported");
}