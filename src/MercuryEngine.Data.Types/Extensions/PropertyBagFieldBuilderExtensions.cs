using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Extensions;

[UsedImplicitly]
internal static class PropertyBagFieldBuilderExtensions
{
	public static PropertyBagFieldBuilder DreadEnum<TEnum>(this PropertyBagFieldBuilder builder, string propertyKey)
	where TEnum : struct, Enum
		=> builder.AddField(propertyKey, () => new DreadEnum<TEnum>());
}