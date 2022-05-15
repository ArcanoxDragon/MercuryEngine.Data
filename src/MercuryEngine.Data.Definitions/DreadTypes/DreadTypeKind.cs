using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MercuryEngine.Data.Definitions.DreadTypes;

[JsonConverter(typeof(StringEnumConverter))]
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