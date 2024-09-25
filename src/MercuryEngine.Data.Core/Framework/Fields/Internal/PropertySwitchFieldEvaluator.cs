using System.Reflection;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Framework.Fields.Internal;

internal class PropertySwitchFieldEvaluator<TCondition, TField>(
	SwitchFieldBuilder<TCondition, TField> builder,
	object owner,
	PropertyInfo property
) : SwitchFieldEvaluator<TCondition, TField>(builder)
where TCondition : notnull
where TField : IBinaryField
{
	private readonly Func<object, TCondition?> getter = ReflectionUtility.GetGetter<TCondition?>(property);

	protected override TCondition GetConditionValue()
	{
		var value = this.getter(owner);

		if (value is null)
			throw new InvalidOperationException($"Property \"{property.Name}\" on \"{owner.GetType().FullName}\" unexpectedly returned null");

		return value;
	}
}