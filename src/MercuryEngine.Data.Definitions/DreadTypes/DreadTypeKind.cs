using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Definitions.DreadTypes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DreadTypeKind
{
	Primitive,
	Struct,
	Enum,
	Typedef,
	Vector,
	Dictionary,
	Pointer,
	Flagset,
	Concrete,
}