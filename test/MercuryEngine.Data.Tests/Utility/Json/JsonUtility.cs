using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal static class JsonUtility
{
	public static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		TypeInfoResolver = JsonTypeInfoResolver.Combine(
			TestJsonSerializerContext.Default,
			new DefaultJsonTypeInfoResolver()
		),
		Converters = {
			new JsonStringEnumConverter(),
			new ArrayFieldJsonConverter(),
			new DictionaryFieldJsonConverter(),
			new NumberFieldJsonConverter<BooleanField, bool>(),
			new NumberFieldJsonConverter<ByteField, byte>(),
			new NumberFieldJsonConverter<CharField, char>(),
			new NumberFieldJsonConverter<Int16Field, short>(),
			new NumberFieldJsonConverter<UInt16Field, ushort>(),
			new NumberFieldJsonConverter<Int32Field, int>(),
			new NumberFieldJsonConverter<UInt32Field, uint>(),
			new NumberFieldJsonConverter<Int64Field, long>(),
			new NumberFieldJsonConverter<UInt64Field, ulong>(),
			new NumberFieldJsonConverter<FloatField, float>(),
			new NumberFieldJsonConverter<DoubleField, double>(),
			new NumberFieldJsonConverter<DecimalField, decimal>(),
			new StrIdJsonConverter(),
			new TerminatedStringFieldJsonConverter(),
			new DreadTypePrefixedFieldJsonConverter(),
			new DreadTypedFieldJsonConverter(),
		},
	};
}