using MercuryEngine.Data.Core.Framework.Fields.Fluent;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Fields;

public abstract class BaseDreadDataStructure<TSelf> : DataStructure<TSelf>
where TSelf : BaseDreadDataStructure<TSelf>
{
	private readonly Lazy<MsePropertyBagField> rawFieldsLazy;

	protected BaseDreadDataStructure()
	{
		this.rawFieldsLazy = new Lazy<MsePropertyBagField>(CreatePropertyBagField);
	}

	public MsePropertyBagField RawFields => this.rawFieldsLazy.Value;

	protected override void Describe(DataStructureBuilder<TSelf> builder)
	{
		builder.RawProperty(m => m.RawFields);
	}

	protected abstract void DefineFields(PropertyBagFieldBuilder fields);

	private MsePropertyBagField CreatePropertyBagField()
		=> MsePropertyBagField.Create(DefineFields);
}