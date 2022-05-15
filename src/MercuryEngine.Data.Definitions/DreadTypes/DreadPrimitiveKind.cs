using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MercuryEngine.Data.Definitions.DreadTypes;

[JsonConverter(typeof(StringEnumConverter))]
public enum DreadPrimitiveKind
{
	Bool,
	Int,
	UInt,
	UInt16,
	UInt64,
	Float,
	String,
	Property,
	Bytes,
	Float_Vec2,
	Float_Vec3,
	Float_Vec4,
}