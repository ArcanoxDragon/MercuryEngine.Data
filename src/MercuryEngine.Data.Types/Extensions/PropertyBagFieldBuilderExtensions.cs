using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Extensions;

internal static class PropertyBagFieldBuilderExtensions
{
	public static PropertyBagFieldBuilder<TStructure> DreadEnum<TStructure, TEnum>(this PropertyBagFieldBuilder<TStructure> builder, string propertyKey, Expression<Func<TStructure, TEnum>> propertyExpression)
	where TStructure : IDataStructure
	where TEnum : struct, Enum
		=> builder.AddPropertyField<TEnum, DreadEnum<TEnum>>(propertyKey, propertyExpression);

	public static PropertyBagFieldBuilder<TStructure> DreadEnum<TStructure, TEnum>(this PropertyBagFieldBuilder<TStructure> builder, string propertyKey, Expression<Func<TStructure, TEnum?>> propertyExpression)
	where TStructure : IDataStructure
	where TEnum : struct, Enum
		=> builder.AddPropertyField<TEnum, DreadEnum<TEnum>>(propertyKey, propertyExpression);
}