using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;

namespace MercuryEngine.Data.Tests.Utility.Json;

internal static class JsonUtility
{
	public static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		IncludeFields = true,
		MaxDepth = 256,
		// ReferenceHandler = ReferenceHandler.Preserve,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		TypeInfoResolver = JsonTypeInfoResolver.Combine(
			TestJsonSerializerContext.Default,
			new DefaultJsonTypeInfoResolver()
		).WithAddedModifier(SortPropertiesByName),
		Converters = {
			new DataMapperJsonConverter(),
			new JsonStringEnumConverter(),
			new AnimatableValueJsonConverter(),
			new ArrayFieldJsonConverter(),
			new DictionaryFieldJsonConverter(),
			new DreadPointerJsonConverter(),
			new DreadTypedFieldJsonConverter(),
			new EnumFieldJsonConverter(),
			new FixedLengthStringFieldJsonConverter(),
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
			new UniformParameterJsonConverter(),
		},
	};

	private static void SortPropertiesByName(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Properties.Count == 0)
			return;

		var propertiesDict = typeInfo.Properties.ToDictionary(p => p.Name);
		var propertiesList = typeInfo.Properties.ToList();
		var maxExistingOrder = propertiesList.Max(p => p.Order);

		propertiesList.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

		foreach (var (i, property) in propertiesList.Pairs())
		{
			if (property.Order != 0)
				// Don't override explicit order
				continue;

			propertiesDict[property.Name].Order = i + maxExistingOrder + 1;
		}
	}
}