using System.Text.Json.Serialization;

namespace MercuryEngine.Data.Definitions.DreadTypes;

[JsonConverter(typeof(JsonStringEnumConverter))]
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