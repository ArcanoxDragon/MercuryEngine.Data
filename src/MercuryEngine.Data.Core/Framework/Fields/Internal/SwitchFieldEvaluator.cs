using MercuryEngine.Data.Core.Framework.Fields.Fluent;

namespace MercuryEngine.Data.Core.Framework.Fields.Internal;

internal interface ISwitchFieldEvaluator<TField>
where TField : IBinaryField
{
	TField GetEffectiveField();
	void ReplaceEffectiveField(TField newField);
}

internal abstract class SwitchFieldEvaluator<TCondition, TField>(SwitchFieldBuilder<TCondition, TField> builder) : ISwitchFieldEvaluator<TField>
where TCondition : notnull
where TField : IBinaryField
{
	private readonly Dictionary<TCondition, TField> fieldMap = builder.Cases;
	private readonly TField?                        fallback = builder.Fallback;

	public TField GetEffectiveField()
	{
		var conditionValue = GetConditionValue();

		if (!this.fieldMap.TryGetValue(conditionValue, out var field))
			return this.fallback ?? throw new InvalidOperationException($"Unknown switch value: {conditionValue}");

		return field;
	}

	public void ReplaceEffectiveField(TField newField)
	{
		var conditionValue = GetConditionValue();

		if (!this.fieldMap.ContainsKey(conditionValue))
			throw new InvalidOperationException($"Unknown switch value: {conditionValue}");

		this.fieldMap[conditionValue] = newField;
	}

	protected abstract TCondition GetConditionValue();
}