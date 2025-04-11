using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

public class DreadArrayField<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TItem
>(string typeName, Func<TItem> itemFactory, List<TItem> initialValue)
	: ArrayField<TItem>(itemFactory, initialValue), ITypedDreadField
where TItem : IBinaryField
{
	public DreadArrayField(string typeName)
		: this(typeName, DefaultItemFactory) { }

	public DreadArrayField(string typeName, Func<TItem> itemFactory)
		: this(typeName, itemFactory, []) { }

	public string TypeName { get; } = typeName;
}