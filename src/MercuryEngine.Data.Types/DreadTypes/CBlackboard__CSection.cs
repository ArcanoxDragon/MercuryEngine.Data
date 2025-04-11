using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
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

	private readonly DictionaryAdapter<TerminatedStringField, DreadPointer<ITypedDreadField>, string, DreadPointer<ITypedDreadField>> sectionsAdapter;

	public CBlackboard__CSection()
	{
		this.sectionsAdapter = new DictionaryAdapter<TerminatedStringField, DreadPointer<ITypedDreadField>, string, DreadPointer<ITypedDreadField>>(
			RawProps,
			bK => bK.Value,
			bV => bV,
			aK => new TerminatedStringField(aK),
			aV => aV
		);
	}

	public IDictionary<string, DreadPointer<ITypedDreadField>> Props => this.sectionsAdapter;

	[StructProperty("dctProps")]
	private IDictionary<TerminatedStringField, DreadPointer<ITypedDreadField>> RawProps
		=> RawFields.Dictionary<TerminatedStringField, DreadPointer<ITypedDreadField>>("dctProps");

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

		IBinaryField? propertyValue = propertyData.Value;

		if (propertyValue is not IBinaryField<T> numericData)
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
		DreadPrimitiveType? primitiveType = null;

		// See if the property already exists in the blackboard with a type implementing the same primitive kind as is being stored
		if (Props.TryGetValue(property, out var existingValue))
		{
			if (existingValue is { Value: IBinaryField<TValue> fieldWithValue })
			{
				// We can reuse it!
				fieldWithValue.Value = value;
				return;
			}

			var candidateType = DreadTypeLibrary.FindType(existingValue.InnerTypeId);

			if (candidateType is DreadPrimitiveType primitive && primitive.PrimitiveKind == primitiveKind)
				primitiveType = primitive;
		}

		// If an existing value was not found, determine which type to use based solely on the primitive kind being stored
		if (primitiveType == null)
		{
			BaseDreadType? candidateType;

			if (BlackboardPrimitiveTypes.TryGetValue(primitiveKind, out string? typeName))
				candidateType = DreadTypeLibrary.FindType(typeName);
			else
				candidateType = DreadTypeLibrary.FindType(type => type is DreadPrimitiveType p && p.PrimitiveKind == primitiveKind);

			if (candidateType is not DreadPrimitiveType primitive)
				throw new ArgumentOutOfRangeException(nameof(primitiveKind), $"Unsupported primitive kind \"{primitiveKind}\"");

			primitiveType = primitive;
		}

		var primitiveField = DreadTypeLibrary.CreateFieldForType(primitiveType);

		if (primitiveField is not (ITypedDreadField and IBinaryField<TValue> primitiveWithValue))
			throw new InvalidOperationException($"Primitive kind \"{primitiveKind}\" is associated with type \"{primitiveType.TypeName}\", which does not have Dread type information");

		primitiveWithValue.Value = value;
		Props[property] = new DreadPointer<ITypedDreadField>((ITypedDreadField) primitiveWithValue);
	}

	#endregion
}