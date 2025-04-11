using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

internal class DreadStringField(string typeName, string initialValue) : TerminatedStringField(initialValue), ITypedDreadField
{
	public DreadStringField(string typeName)
		: this(typeName, string.Empty) { }

	public string TypeName { get; } = typeName;
}