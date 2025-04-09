using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Fields;

[PublicAPI]
public abstract class BaseDreadDataStructure<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	TSelf
> : DataStructure<TSelf>
where TSelf : BaseDreadDataStructure<TSelf>
{
	private static readonly ConcurrentDictionary<Type, MsePropertyBagField> PropertyBagPrototypes = [];

	private readonly Lazy<MsePropertyBagField> rawFieldsLazy;

	protected BaseDreadDataStructure()
	{
		this.rawFieldsLazy = new Lazy<MsePropertyBagField>(CreatePropertyBagField);
	}

	[JsonIgnore]
	public MsePropertyBagField RawFields => this.rawFieldsLazy.Value;

	protected override void Describe(DataStructureBuilder<TSelf> builder)
	{
		builder.RawProperty(m => m.RawFields);
	}

	protected abstract void DefineFields(PropertyBagFieldBuilder fields);

	private MsePropertyBagField CreatePropertyBagField()
	{
		var prototype = PropertyBagPrototypes.GetOrAdd(GetType(), _ => CreatePropergyBagPrototype());

		return prototype.Clone();
	}

	private MsePropertyBagField CreatePropergyBagPrototype()
		=> MsePropertyBagField.Create(DefineFields);
}