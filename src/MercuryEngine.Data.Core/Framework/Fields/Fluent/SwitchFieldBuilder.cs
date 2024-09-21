using JetBrains.Annotations;

namespace MercuryEngine.Data.Core.Framework.Fields.Fluent;

[PublicAPI]
public class SwitchFieldBuilder<TCondition, TField>
where TCondition : notnull
where TField : IBinaryField
{
	internal SwitchFieldBuilder() { }

	internal Dictionary<TCondition, TField> Cases { get; } = [];

	internal TField? Fallback { get; private set; }

	public SwitchFieldBuilder<TCondition, TField> AddCase(TCondition caseValue, TField field)
	{
		if (!Cases.TryAdd(caseValue, field))
			throw new ArgumentException($"A case for the value \"{caseValue}\" has already been added", nameof(caseValue));

		return this;
	}

	public SwitchFieldBuilder<TCondition, TField> AddFallback(TField field)
	{
		Fallback = field;
		return this;
	}
}