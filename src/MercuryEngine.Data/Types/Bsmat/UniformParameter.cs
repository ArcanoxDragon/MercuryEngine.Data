using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bsmat;

public class UniformParameter : DataStructure<UniformParameter>
{
	public const char TypeFloat       = 'f';
	public const char TypeSignedInt   = 'i';
	public const char TypeUnsignedInt = 'u';

	public UniformParameter()
	{
		ValuesField = SwitchField.FromProperty(this, m => m.Type, builder => {
			builder.AddCase(TypeFloat, FloatsField);
			builder.AddCase(TypeSignedInt, SignedIntsField);
			builder.AddCase(TypeUnsignedInt, UnsignedIntsField);
			builder.AddFallback(FloatsField);
		});
	}

	public string Name { get; set; } = string.Empty;

	public char Type { get; private set; } = TypeFloat;

	[JsonIgnore]
	public float[] FloatValues
	{
		get => CheckTypeAndGetValues<float, FloatField>(TypeFloat, FloatsField);
		set => SetTypeAndValues(TypeFloat, FloatsField, value);
	}

	[JsonIgnore]
	public int[] SignedIntValues
	{
		get => CheckTypeAndGetValues<int, Int32Field>(TypeSignedInt, SignedIntsField);
		set => SetTypeAndValues(TypeSignedInt, SignedIntsField, value);
	}

	[JsonIgnore]
	public uint[] UnsignedIntValues
	{
		get => CheckTypeAndGetValues<uint, UInt32Field>(TypeUnsignedInt, UnsignedIntsField);
		set => SetTypeAndValues(TypeUnsignedInt, UnsignedIntsField, value);
	}

	#region Private Data

	private ArrayField<FloatField>  FloatsField       { get; } = new();
	private ArrayField<Int32Field>  SignedIntsField   { get; } = new();
	private ArrayField<UInt32Field> UnsignedIntsField { get; } = new();

	private SwitchField<IBinaryField> ValuesField { get; }

	#endregion

	protected override void Describe(DataStructureBuilder<UniformParameter> builder)
	{
		builder.Property(m => m.Name);
		builder.Property(m => m.Type);
		builder.RawProperty(m => m.ValuesField);
	}

	private T[] CheckTypeAndGetValues<T, TField>(char expectedType, ArrayField<TField> storageField, [CallerMemberName] string? propertyName = null)
	where T : notnull
	where TField : IBinaryField<T>
	{
		if (Type != expectedType)
			throw new InvalidOperationException($"Cannot read property \"{propertyName}\" because the {nameof(Type)} is not '{expectedType}'");

		return storageField.Value.Select(item => item.Value).ToArray();
	}

	private void SetTypeAndValues<T, TField>(char type, ArrayField<TField> storageField, T[] values)
	where T : notnull
	where TField : IBinaryField<T>, new()
	{
		Type = type;
		storageField.Value.Clear();
		storageField.Value.AddRange(values.Select(value => new TField { Value = value }));
	}
}