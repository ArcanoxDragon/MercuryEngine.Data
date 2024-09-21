namespace MercuryEngine.Data.Core.Framework.Fields.Internal;

internal interface ISwitchFieldEvaluator<TField>
where TField : IBinaryField
{
	TField GetEffectiveField();
	void ReplaceEffectiveField(TField newField);
}

internal class SwitchFieldEvaluator<TCondition, TField>(
	Dictionary<TCondition, TField> fieldMap,
	TField? fallback,
	Func<TCondition> getConditionValue
) : ISwitchFieldEvaluator<TField>
where TCondition : notnull
where TField : IBinaryField
{
	private readonly Dictionary<TCondition, TField> fieldMap          = fieldMap;
	private readonly TField?                        fallback          = fallback;
	private readonly Func<TCondition>               getConditionValue = getConditionValue;

	public TField GetEffectiveField()
	{
		var conditionValue = this.getConditionValue();

		if (!this.fieldMap.TryGetValue(conditionValue, out var field))
			return this.fallback ?? throw new InvalidOperationException($"Unknown switch value: {conditionValue}");

		return field;
	}

	public void ReplaceEffectiveField(TField newField)
	{
		var conditionValue = this.getConditionValue();

		if (!this.fieldMap.ContainsKey(conditionValue))
			throw new InvalidOperationException($"Unknown switch value: {conditionValue}");

		this.fieldMap[conditionValue] = newField;
	}
}