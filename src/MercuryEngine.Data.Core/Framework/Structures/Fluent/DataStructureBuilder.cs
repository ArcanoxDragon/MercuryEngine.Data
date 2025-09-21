using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.FieldHandlers;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Structures.Fluent;

/// <summary>
/// Provides a fluent interface for defining the format of a <see cref="DataStructure{T}"/>.
/// </summary>
/// <typeparam name="T">The type of structure this <see cref="DataStructureBuilder{T}"/> can build.</typeparam>
[PublicAPI]
public sealed class DataStructureBuilder<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	T
>
where T : IDataStructure
{
	internal DataStructureBuilder() { }

	private List<DataStructureField> Fields { get; init; } = [];

	internal List<DataStructureField> Build() => Fields;

	public DataStructureBuilder<TOther> For<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
		TOther
	>()
	where TOther : T
		=> new() {
			Fields = Fields, // Build to same field collection
		};

	#region Constant Fields

	public DataStructureBuilder<T> Constant(string text, string? description = null, bool assertValueOnRead = true, bool terminated = true)
	{
		return AddConstantField(() => {
			return terminated ? new TerminatedStringField(text) : new FixedLengthStringField(text);
		}, text, description, assertValueOnRead);
	}

	public DataStructureBuilder<T> Constant(bool value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new BooleanField(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(byte value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new ByteField(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(char value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new CharField(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(short value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new Int16Field(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(ushort value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new UInt16Field(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(int value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new Int32Field(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(uint value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new UInt32Field(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(long value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new Int64Field(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(ulong value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new UInt64Field(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(float value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new FloatField(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(double value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new DoubleField(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant(decimal value, string? description = null, bool assertValueOnRead = true)
		=> AddConstantField(() => new DecimalField(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Constant<TEnum>(TEnum value, string? description = null, bool assertValueOnRead = true)
	where TEnum : struct, Enum
		=> AddConstantField(() => new EnumField<TEnum>(value), value, description, assertValueOnRead);

	public DataStructureBuilder<T> Padding(uint length, byte paddingByte = 0)
		=> AddConstantField(() => new PaddingField(length, paddingByte), $"<padding: {length} bytes>");

	public DataStructureBuilder<T> AddConstantField<TField>(Func<TField> fieldFactory, string? description = null)
	where TField : IBinaryField
		=> AddField(new DataStructureField(new ConstantValueFieldHandler(() => fieldFactory()), description ?? $"<constant: {typeof(TField).GetDisplayName()}>"));

	public DataStructureBuilder<T> AddConstantField<TValue>(Func<IBinaryField<TValue>> fieldFactory, TValue expectedValue, string? description = null, bool assertValueOnRead = true)
	where TValue : notnull
		=> AddField(new DataStructureField(new ConstantValueFieldHandler<TValue>(fieldFactory, expectedValue, assertValueOnRead), description ?? $"<constant: {typeof(TValue).GetDisplayName()} = {expectedValue}>"));

	#endregion

	#region Property Fields

	public DataStructureBuilder<T> Property(Expression<Func<T, string?>> propertyExpression)
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var nullabilityInfo = ReflectionUtility.NullabilityInfoContext.Create(property);

		if (nullabilityInfo.WriteState == NullabilityState.NotNull)
			// Property is annotated as non-null, so we will use Property instead of NullableProperty
			// to ensure that "null" is not written during reading
			return Property(propertyExpression!, _ => new TerminatedStringField());

		return NullableProperty(propertyExpression, _ => new TerminatedStringField());
	}

	public DataStructureBuilder<T> Property(Expression<Func<T, bool>> propertyExpression)
		=> Property(propertyExpression, _ => new BooleanField());

	public DataStructureBuilder<T> Property(Expression<Func<T, bool?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new BooleanField());

	public DataStructureBuilder<T> Property(Expression<Func<T, byte>> propertyExpression)
		=> Property(propertyExpression, _ => new ByteField());

	public DataStructureBuilder<T> Property(Expression<Func<T, byte?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new ByteField());

	public DataStructureBuilder<T> Property(Expression<Func<T, char>> propertyExpression)
		=> Property(propertyExpression, _ => new CharField());

	public DataStructureBuilder<T> Property(Expression<Func<T, char?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new CharField());

	public DataStructureBuilder<T> Property(Expression<Func<T, short>> propertyExpression)
		=> Property(propertyExpression, _ => new Int16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, short?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new Int16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort>> propertyExpression)
		=> Property(propertyExpression, _ => new UInt16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ushort?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new UInt16Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, int>> propertyExpression)
		=> Property(propertyExpression, _ => new Int32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, int?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new Int32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, uint>> propertyExpression)
		=> Property(propertyExpression, _ => new UInt32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, uint?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new UInt32Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, long>> propertyExpression)
		=> Property(propertyExpression, _ => new Int64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, long?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new Int64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong>> propertyExpression)
		=> Property(propertyExpression, _ => new UInt64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, ulong?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new UInt64Field());

	public DataStructureBuilder<T> Property(Expression<Func<T, float>> propertyExpression)
		=> Property(propertyExpression, _ => new FloatField());

	public DataStructureBuilder<T> Property(Expression<Func<T, float?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new FloatField());

	public DataStructureBuilder<T> Property(Expression<Func<T, double>> propertyExpression)
		=> Property(propertyExpression, _ => new DoubleField());

	public DataStructureBuilder<T> Property(Expression<Func<T, double?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new DoubleField());

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal>> propertyExpression)
		=> Property(propertyExpression, _ => new DecimalField());

	public DataStructureBuilder<T> Property(Expression<Func<T, decimal?>> propertyExpression)
		=> NullableProperty(propertyExpression, _ => new DecimalField());

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum>> propertyExpression)
	where TEnum : struct, Enum
		=> Property(propertyExpression, _ => new EnumField<TEnum>());

	public DataStructureBuilder<T> Property<TEnum>(Expression<Func<T, TEnum?>> propertyExpression)
	where TEnum : struct, Enum
		=> NullableProperty(propertyExpression, _ => new EnumField<TEnum>());

	public DataStructureBuilder<T> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression, Func<T, IBinaryField<TProperty>> fieldFactory)
	where TProperty : notnull
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<T, TProperty>(fieldFactory, property);
		var description = $"{property.Name}: {typeof(TProperty).GetDisplayName()}";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> NullableProperty<TProperty>(Expression<Func<T, TProperty?>> propertyExpression, Func<T, IBinaryField<TProperty>> fieldFactory)
	where TProperty : class
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<T, TProperty>(fieldFactory, property, nullable: true);
		var description = $"{property.Name}: {typeof(TProperty).GetDisplayName()}?";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> NullableProperty<TProperty>(Expression<Func<T, TProperty?>> propertyExpression, Func<T, IBinaryField<TProperty>> fieldFactory)
	where TProperty : struct
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ValuePropertyFieldHandler<T, TProperty>(fieldFactory, property, nullable: true);
		var description = $"{property.Name}: {typeof(TProperty).GetDisplayName()}?";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	#region Raw Properties

	public DataStructureBuilder<T> RawProperty<TField>(Expression<Func<T, TField>> propertyExpression)
	where TField : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new DirectPropertyFieldHandler<T>(property);
		var description = $"{property.Name}: {typeof(TField).GetDisplayName()}";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> NullableRawProperty<TField>(Expression<Func<T, TField?>> propertyExpression)
	where TField : class, IBinaryField, new()
		=> NullableRawProperty(propertyExpression, _ => new TField());

	public DataStructureBuilder<T> NullableRawProperty<TField>(Expression<Func<T, TField?>> propertyExpression, Func<T, TField> fieldFactory)
	where TField : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new NullableDirectPropertyFieldHandler<T, TField>(property, fieldFactory);
		var description = $"{property.Name}: {typeof(TField).GetDisplayName()}?";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	#region Collection Properties

	public DataStructureBuilder<T> Array<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TItem
	>(Expression<Func<T, IList<TItem>>> propertyExpression)
	where TItem : class, IBinaryField, new()
		=> Array(propertyExpression, _ => new TItem());

	public DataStructureBuilder<T> Array<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TItem
	>(Expression<Func<T, IList<TItem>>> propertyExpression, Func<T, TItem> itemFactory)
	where TItem : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new ArrayPropertyFieldHandler<T, TItem>(owner => new ArrayField<TItem>(() => itemFactory(owner)), property);
		var description = $"{property.Name}: {typeof(TItem).GetDisplayName()}[]";
		return AddField(new DataStructureField(adapter, description));
	}

	public DataStructureBuilder<T> Dictionary<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TKey,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TValue
	>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression)
	where TKey : class, IBinaryField, new()
	where TValue : class, IBinaryField, new()
		=> Dictionary(propertyExpression, _ => new TKey(), _ => new TValue());

	public DataStructureBuilder<T> Dictionary<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TKey,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		TValue
	>(Expression<Func<T, IDictionary<TKey, TValue>>> propertyExpression, Func<T, TKey> keyFactory, Func<T, TValue> valueFactory)
	where TKey : class, IBinaryField
	where TValue : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new DictionaryPropertyFieldHandler<T, TKey, TValue>(
			owner => new DictionaryField<TKey, TValue>(() => keyFactory(owner), () => valueFactory(owner)),
			property);
		var description = $"{property.Name}: Dictionary<{typeof(TKey).GetDisplayName()}, {typeof(TValue).GetDisplayName()}>";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	#region Pointer Properties

	public DataStructureBuilder<T> Pointer<TField>(Expression<Func<T, TField?>> propertyExpression, uint startByteAlignment = 0, uint endByteAlignment = 0, bool unique = false)
	where TField : class, IBinaryField, new()
		=> Pointer(propertyExpression, _ => new TField(), startByteAlignment, endByteAlignment, unique);

	public DataStructureBuilder<T> Pointer<TField>(Expression<Func<T, TField?>> propertyExpression, Func<T, TField> fieldFactory, uint startByteAlignment = 0, uint endByteAlignment = 0, bool unique = false)
	where TField : class, IBinaryField
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var adapter = new PointerPropertyFieldHandler<T, TField>(property, fieldFactory, startByteAlignment, endByteAlignment, unique);
		var description = $"{property.Name}: {typeof(TField).GetDisplayName()}*";
		return AddField(new DataStructureField(adapter, description));
	}

	#endregion

	public DataStructureBuilder<T> AddField(DataStructureField field)
	{
		Fields.Add(field);
		return this;
	}
}