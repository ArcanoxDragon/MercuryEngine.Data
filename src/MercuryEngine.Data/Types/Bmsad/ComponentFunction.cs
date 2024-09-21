using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Bmsad;

public class ComponentFunction : DataStructure<ComponentFunction>
{
	public ComponentFunction()
	{
		Arguments = new DictionaryAdapter<StrId, FunctionArgument, string, FunctionArgument>(
			RawArguments,
			bk => bk.StringValue,
			bv => bv,
			ak => ak,
			av => av
		);
	}

	public string Name     { get; set; } = string.Empty;
	public bool   Unknown1 { get; set; }
	public bool   Unknown2 { get; set; }

	public IDictionary<string, FunctionArgument> Arguments { get; }

	private Dictionary<StrId, FunctionArgument> RawArguments { get; } = [];

	protected override void Describe(DataStructureBuilder<ComponentFunction> builder)
		=> builder
			.Property(m => m.Name)
			.Property(m => m.Unknown1)
			.Property(m => m.Unknown2)
			.Dictionary(m => m.RawArguments);
}