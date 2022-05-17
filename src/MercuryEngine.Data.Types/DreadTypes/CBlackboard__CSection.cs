using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.DataTypes;

namespace MercuryEngine.Data.Types.DreadTypes;

public partial class CBlackboard__CSection
{
	public Dictionary<string, TypedDreadDataType> Props { get; private set; } = new();

	[StructProperty("dctProps")]
	private Dictionary<TerminatedStringDataType, TypedDreadDataType> RawProps
	{
		get => Props.ToDictionary(pair => new TerminatedStringDataType(pair.Key), pair => pair.Value);
		set => Props = value.ToDictionary(pair => pair.Key.Value, pair => pair.Value);
	}
}