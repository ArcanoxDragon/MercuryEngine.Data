using System.Linq.Expressions;
using System.Reflection;
using MercuryEngine.Data.Framework.Components;
using MercuryEngine.Data.Utility;

namespace MercuryEngine.Data.Framework.Adapters;

public class PropertyComponentAdapter<TTarget, TProperty> : IComponentAdapter<IBinaryComponent<TProperty>>
where TTarget : class
where TProperty : notnull
{
	private readonly PropertyInfo propertyInfo;

	public PropertyComponentAdapter(IBinaryComponent<TProperty> component, TTarget target, Expression<Func<TTarget, TProperty>> propertyExpression)
	{
		Target = target;
		Component = component;

		this.propertyInfo = ExpressionUtility.GetProperty(propertyExpression);

		if (!this.propertyInfo.CanRead || !this.propertyInfo.CanWrite)
			throw new ArgumentException("A property must have both a getter and a setter in order to be used as a component field");
	}

	public TTarget                     Target    { get; }
	public IBinaryComponent<TProperty> Component { get; }

	public void Read(BinaryReader reader)
	{
		var value = Component.Read(reader);

		this.propertyInfo.SetValue(Target, value);
	}

	public void Write(BinaryWriter writer)
	{
		var value = (TProperty?) this.propertyInfo.GetValue(Target);

		if (value is null)
			throw new InvalidOperationException($"The value of \"{this.propertyInfo.Name}\" was null on the target object while trying to write data.");

		Component.Write(writer, value);
	}
}