using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Bmsad;

public class ComponentFunction : DataStructure<ComponentFunction>
{
	private static readonly Expression<Func<ComponentFunction, string?>> NameExpression     = m => m.Name;
	private static readonly Expression<Func<ComponentFunction, bool>>    Unknown1Expression = m => m.Unknown1;
	private static readonly Expression<Func<ComponentFunction, bool>>    Unknown2Expression = m => m.Unknown2;

	private static readonly Expression<Func<ComponentFunction, IDictionary<StrId, FunctionArgument>>> RawArgumentsExpression = m => m.RawArguments;

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
	{
		builder.Property(NameExpression);
		builder.Property(Unknown1Expression);
		builder.Property(Unknown2Expression);
		builder.Dictionary(RawArgumentsExpression);
	}
}