using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Definitions.DreadTypes;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.DataTypes;

namespace MercuryEngine.Data.Types.DreadTypes;

[PublicAPI]
public partial class CBlackboard__CSection
{
	public Dictionary<string, TypedDreadDataType> Props { get; private set; } = new();

	[StructProperty("dctProps")]
	private Dictionary<TerminatedStringDataType, TypedDreadDataType> RawProps
	{
		get => Props.ToDictionary(pair => new TerminatedStringDataType(pair.Key), pair => pair.Value);
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

		if (propertyData.InnerData is not IBinaryDataType<T> numericData)
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
		var primitiveType = DreadTypeRegistry.FindType(type => type is DreadPrimitiveType primitive && primitive.PrimitiveKind == primitiveKind);

		if (primitiveType is null)
			throw new ArgumentOutOfRangeException(nameof(primitiveKind), $"Unsupported primitive kind \"{primitiveKind}\"");

		var primitiveValue = new TypedDreadValue(primitiveType);

		if (primitiveValue.Data is not IBinaryDataType<TValue> primitiveData)
			throw new InvalidOperationException($"Primitive kind \"{primitiveKind}\" is associated with type \"{primitiveType.TypeName}\", which does not support values of type \"{typeof(TValue).Name}\"");

		primitiveData.Value = value;

		Props[property] = new TypedDreadDataType(primitiveValue);
	}

	#endregion
}