using MercuryEngine.Data.Core.Framework.Fields;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MercuryEngine.Data.Test.Utility;

internal static class JsonUtility
{
	public static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		Converters = {
			new JsonStringEnumConverter(),
			new ArrayFieldJsonConverter(),
			new DictionaryFieldJsonConverter(),
			new NumberFieldJsonConverter<BooleanField, bool>(),
			new NumberFieldJsonConverter<Int16Field, short>(),
			new NumberFieldJsonConverter<UInt16Field, ushort>(),
			new NumberFieldJsonConverter<Int32Field, int>(),
			new NumberFieldJsonConverter<UInt32Field, uint>(),
			new NumberFieldJsonConverter<Int64Field, long>(),
			new NumberFieldJsonConverter<UInt64Field, ulong>(),
			new NumberFieldJsonConverter<FloatField, float>(),
			new NumberFieldJsonConverter<DoubleField, double>(),
			new NumberFieldJsonConverter<DecimalField, decimal>(),
			new TerminatedStringFieldJsonConverter(),
			new DreadTypePrefixedFieldJsonConverter(),
		},
	};
}