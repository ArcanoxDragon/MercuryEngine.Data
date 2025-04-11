using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

public class DreadDictionaryField<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TKey,
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TValue
>(string typeName, Func<TKey> keyFactory, Func<TValue> valueFactory)
	: DictionaryField<TKey, TValue>(keyFactory, valueFactory), ITypedDreadField
where TKey : IBinaryField
where TValue : IBinaryField
{
	public DreadDictionaryField(string typeName)
		: this(typeName, DefaultKeyFactory, DefaultValueFactory) { }

	public string TypeName { get; } = typeName;
}