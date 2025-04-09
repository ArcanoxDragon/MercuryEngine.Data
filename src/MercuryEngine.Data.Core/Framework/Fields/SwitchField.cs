using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Framework.Fields.Internal;
using MercuryEngine.Data.Core.Utility;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Core.Framework.Fields;

[PublicAPI]
public class SwitchField<TField> : IBinaryField
where TField : IBinaryField
{
	#region Static Factories

	public static SwitchField<TField> FromProperty<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
		TOwner,
		TProperty
	>(
		TOwner @object,
		Expression<Func<TOwner, TProperty>> propertyExpression,
		Action<SwitchFieldBuilder<TProperty, TField>> build
	)
	where TOwner : notnull
	where TProperty : notnull
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var builder = new SwitchFieldBuilder<TProperty, TField>();

		build(builder);

		var evaluator = new PropertySwitchFieldEvaluator<TOwner, TProperty, TField>(builder, @object, property);

		return new SwitchField<TField>(evaluator);
	}

	#endregion

	private readonly ISwitchFieldEvaluator<TField> evaluator;

	internal SwitchField(ISwitchFieldEvaluator<TField> evaluator)
	{
		this.evaluator = evaluator;
	}

	public TField EffectiveField
	{
		get => this.evaluator.GetEffectiveField();
		set => this.evaluator.ReplaceEffectiveField(value);
	}

	[JsonIgnore]
	public uint Size => EffectiveField.Size;

	public void Reset() => ( EffectiveField as IResettableField )?.Reset();
	public void Read(BinaryReader reader) => EffectiveField.Read(reader);
	public void Write(BinaryWriter writer) => EffectiveField.Write(writer);

	public Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
		=> EffectiveField.ReadAsync(reader, cancellationToken);

	public Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
		=> EffectiveField.WriteAsync(writer, cancellationToken);
}

[PublicAPI]
public class SwitchField : SwitchField<IBinaryField>
{
	#region Static Factories

	public static new SwitchField FromProperty<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
		TOwner,
		TProperty
	>(
		TOwner @object,
		Expression<Func<TOwner, TProperty>> propertyExpression,
		Action<SwitchFieldBuilder<TProperty, IBinaryField>> build
	)
	where TOwner : notnull
	where TProperty : notnull
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);
		var builder = new SwitchFieldBuilder<TProperty, IBinaryField>();

		build(builder);

		var evaluator = new PropertySwitchFieldEvaluator<TOwner, TProperty, IBinaryField>(builder, @object, property);

		return new SwitchField(evaluator);
	}

	#endregion

	internal SwitchField(ISwitchFieldEvaluator<IBinaryField> evaluator)
		: base(evaluator) { }
}