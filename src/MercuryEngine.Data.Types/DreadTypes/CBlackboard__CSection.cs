using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

[PublicAPI]
public partial class CBlackboard__CSection
{
	/// <summary>
	/// Used to resolve the correct primitive type to use when writing certain data types to Blackboard sections.
	/// The primitive kinds listed in this dictionary may have more than one type that implements that kind, and
	/// picking the wrong implementation may cause the Blackboard to not be loadable by the game.
	/// </summary>
	private static readonly Dictionary<DreadPrimitiveKind, string> BlackboardPrimitiveTypes = new() {
		{ DreadPrimitiveKind.UInt, "unsigned_int" },
		{ DreadPrimitiveKind.UInt64, "unsigned_long" },
		{ DreadPrimitiveKind.Float, "float" },
		{ DreadPrimitiveKind.String, "base::global::TRntString256" },
	};

	public Dictionary<string, DreadTypePrefixedField> Props { get; private set; } = [];

	[StructProperty("dctProps")]
	private Dictionary<TerminatedStringField, DreadTypePrefixedField> RawProps
	{
		get => Props.ToDictionary(pair => new TerminatedStringField(pair.Key), pair => pair.Value);
		set => Props = value.ToDictionary(pair => pair.Key.Value, pair => pair.Value);
	}

	#region Property Getters

	public bool TryGetString(string property, [MaybeNullWhen(false)] out string value)
		=> TryGetSimpleValue(property, out value);

	public bool TryGetBoolean(string property, out bool value) => TryGetSimpleValue(property, out value);
	public bool TryGetUInt16(string property, out ushort value) => TryGetSimpleValue(property, out value);
	public bool TryGetInt32(string property, out int value) => TryGetSimpleValue(property, out value);
	public bool TryGetUInt32(string property, out uint value) => TryGetSimpleValue(property, out value);
	public bool TryGetUInt64(string property, out ulong value) => TryGetSimpleValue(property, out value);
	public bool TryGetFloat(string property, out float value) => TryGetSimpleValue(property, out value);

	public bool TryGetEnum<T>(string property, out T value)
	where T : struct, Enum
		=> TryGetSimpleValue(property, out value);

	private bool TryGetSimpleValue<T>(string property, [MaybeNullWhen(false)] out T value)
	where T : notnull
	{
		value = default;

		if (!Props.TryGetValue(property, out var propertyData))
			return false;

		if (propertyData.InnerData is not IBinaryField<T> numericData)
			return false;

		value = numericData.Value;
		return true;
	}

	#endregion

	#region Property Setters

	public void PutValue(string property, string value) => PutPrimitiveValue(DreadPrimitiveKind.String, property, value);
	public void PutValue(string property, bool value) => PutPrimitiveValue(DreadPrimitiveKind.Bool, property, value);
	public void PutValue(string property, ushort value) => PutPrimitiveValue(DreadPrimitiveKind.UInt16, property, value);
	public void PutValue(string property, int value) => PutPrimitiveValue(DreadPrimitiveKind.Int, property, value);
	public void PutValue(string property, uint value) => PutPrimitiveValue(DreadPrimitiveKind.UInt, property, value);
	public void PutValue(string property, ulong value) => PutPrimitiveValue(DreadPrimitiveKind.UInt64, property, value);
	public void PutValue(string property, float value) => PutPrimitiveValue(DreadPrimitiveKind.Float, property, value);

	private void PutPrimitiveValue<TValue>(DreadPrimitiveKind primitiveKind, string property, TValue value)
	where TValue : notnull
	{
		BaseDreadType? primitiveType = null;

		// See if the property already exists in the blackboard with a type implementing the same primitive kind as is being stored
		if (Props.TryGetValue(property, out var existingValue))
		{
			var candidateType = DreadTypeRegistry.FindType(existingValue.InnerTypeId);

			if (candidateType is DreadPrimitiveType primitive && primitive.PrimitiveKind == primitiveKind)
				primitiveType = candidateType;
		}

		// If an existing type was not found, determine which type to use based solely on the primitive kind being stored
		if (primitiveType == null)
		{
			if (BlackboardPrimitiveTypes.TryGetValue(primitiveKind, out string? typeName))
				primitiveType = DreadTypeRegistry.FindType(typeName);
			else
				primitiveType = DreadTypeRegistry.FindType(type => type is DreadPrimitiveType primitive && primitive.PrimitiveKind == primitiveKind);
		}

		if (primitiveType is null)
			throw new ArgumentOutOfRangeException(nameof(primitiveKind), $"Unsupported primitive kind \"{primitiveKind}\"");

		if (DreadTypeRegistry.GetFieldForType(primitiveType) is not (ITypedDreadField and IBinaryField<TValue> typedField))
			throw new NotSupportedException($"Primitive type \"{primitiveType.TypeName}\" resulted in a field that was not {nameof(ITypedDreadField)}");

		typedField.Value = value;

		Props[property] = new DreadTypePrefixedField((ITypedDreadField) typedField);
	}

	#endregion
}