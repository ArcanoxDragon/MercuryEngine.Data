using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Types.Utility;

namespace MercuryEngine.Data.Types.Fields;

public class MsePropertyBagField : PropertyBagField<StrId>
{
	public static MsePropertyBagField Create(Action<PropertyBagFieldBuilder> defineFields)
	{
		PropertyBagFieldBuilder builder = new();

		defineFields(builder);

		return new MsePropertyBagField(builder);
	}

	public MsePropertyBagField(IReadOnlyDictionary<string, Func<IBinaryField>> fieldDefinitions)
		: base(fieldDefinitions, CrcPropertyKeyTranslator.Instance, StrId.EqualityComparer) { }

	public MsePropertyBagField(PropertyBagFieldBuilder builder)
		: base(builder, CrcPropertyKeyTranslator.Instance, StrId.EqualityComparer) { }
}