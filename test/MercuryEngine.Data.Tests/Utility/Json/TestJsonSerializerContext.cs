using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Formats;

namespace MercuryEngine.Data.Tests.Utility.Json;

[JsonSourceGenerationOptions(
	WriteIndented = true,
	Converters = [
		typeof(JsonStringEnumConverter),
		typeof(ArrayFieldJsonConverter),
		typeof(DictionaryFieldJsonConverter),
		typeof(DreadPointerJsonConverter),
		typeof(DreadTypedFieldJsonConverter),
		typeof(EnumFieldJsonConverter),
		typeof(FixedLengthStringFieldJsonConverter),
		typeof(NumberFieldJsonConverter<BooleanField, bool>),
		typeof(NumberFieldJsonConverter<ByteField, byte>),
		typeof(NumberFieldJsonConverter<CharField, char>),
		typeof(NumberFieldJsonConverter<Int16Field, short>),
		typeof(NumberFieldJsonConverter<UInt16Field, ushort>),
		typeof(NumberFieldJsonConverter<Int32Field, int>),
		typeof(NumberFieldJsonConverter<UInt32Field, uint>),
		typeof(NumberFieldJsonConverter<Int64Field, long>),
		typeof(NumberFieldJsonConverter<UInt64Field, ulong>),
		typeof(NumberFieldJsonConverter<FloatField, float>),
		typeof(NumberFieldJsonConverter<DoubleField, double>),
		typeof(NumberFieldJsonConverter<DecimalField, decimal>),
		typeof(StrIdJsonConverter),
		typeof(TerminatedStringFieldJsonConverter),
	]
)]
[JsonSerializable(typeof(Bmsad))]
[JsonSerializable(typeof(Bmssv))]
[JsonSerializable(typeof(Bptdat))]
[JsonSerializable(typeof(TerminatedStringField), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(FixedLengthStringField), GenerationMode = JsonSourceGenerationMode.Metadata)]
public partial class TestJsonSerializerContext : JsonSerializerContext;