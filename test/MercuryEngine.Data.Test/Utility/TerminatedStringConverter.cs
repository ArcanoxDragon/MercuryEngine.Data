using System;
using MercuryEngine.Data.Core.Framework.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MercuryEngine.Data.Test.Utility;

public class TerminatedStringConverter : JsonConverter<TerminatedStringDataType>
{
	public override bool CanRead => false;

	public override void WriteJson(JsonWriter writer, TerminatedStringDataType? value, JsonSerializer serializer)
	{
		if (value is null)
		{
			writer.WriteNull();
			return;
		}

		var valueToken = JToken.FromObject(value.Value);

		valueToken.WriteTo(writer);
	}

	public override TerminatedStringDataType ReadJson(JsonReader reader, Type objectType, TerminatedStringDataType? existingValue, bool hasExistingValue, JsonSerializer serializer)
		=> throw new NotSupportedException();
}