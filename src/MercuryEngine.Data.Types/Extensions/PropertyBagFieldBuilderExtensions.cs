using System.Linq.Expressions;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Extensions;

[UsedImplicitly]
internal static class PropertyBagFieldBuilderExtensions
{
	public static PropertyBagFieldBuilder<T> DreadEnum<T, TEnum>(this PropertyBagFieldBuilder<T> builder, string propertyKey, Expression<Func<T, TEnum>> propertyExpression)
	where T : DataStructure<T>, IDescribeDataStructure<T>
	where TEnum : struct, Enum
		=> builder.Property(propertyKey, propertyExpression, new DreadEnum<TEnum>());

	public static PropertyBagFieldBuilder<T> DreadEnum<T, TEnum>(this PropertyBagFieldBuilder<T> builder, string propertyKey, Expression<Func<T, TEnum?>> propertyExpression)
	where T : DataStructure<T>, IDescribeDataStructure<T>
	where TEnum : struct, Enum
		=> builder.NullableProperty(propertyKey, propertyExpression, new DreadEnum<TEnum>());
}