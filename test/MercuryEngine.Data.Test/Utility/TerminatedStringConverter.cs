using System;
using MercuryEngine.Data.Core.Framework.DataTypes;
using Newtonsoft.Json;

namespace MercuryEngine.Data.Test.Utility;

public class TerminatedStringConverter : JsonConverter<TerminatedStringDataType>
{
	public override bool CanRead => false;

	public override void WriteJson(JsonWriter writer, TerminatedStringDataType? value, JsonSerializer serializer)
	{
		if (value is null)
			writer.WriteNull();
		else
			writer.WriteValue(value.Value);
	}

	public override TerminatedStringDataType ReadJson(JsonReader reader, Type objectType, TerminatedStringDataType? existingValue, bool hasExistingValue, JsonSerializer serializer)
		=> throw new NotSupportedException();
}