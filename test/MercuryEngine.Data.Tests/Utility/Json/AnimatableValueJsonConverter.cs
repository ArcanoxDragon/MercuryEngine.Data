using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Types.Bcskla;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal class AnimatableValueJsonConverter : JsonConverter<AnimatableValue>
{
	public override void Write(Utf8JsonWriter writer, AnimatableValue value, JsonSerializerOptions options)
	{
		var jsonObject = new JsonObject();

		if (value.IsConstant)
		{
			jsonObject["ConstantValue"] = value.ConstantValue;
		}
		else
		{
			var valuesDict = new JsonObject();

			foreach (var (frame, keyframeValue) in value.GetValues())
			{
				valuesDict[frame.ToString()] = new JsonObject {
					["Value"] = keyframeValue.Value,
					["Rate"] = keyframeValue.Rate,
				};
			}

			jsonObject["Values"] = valuesDict;
		}

		jsonObject.WriteTo(writer, options);
	}

	public override AnimatableValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException("Reading is not supported");
}