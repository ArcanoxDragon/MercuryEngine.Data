using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Utility.DreadTypeHelpers;

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