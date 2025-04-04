using JetBrains.Annotations;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmssv : BaseStandardFormat<Bmssv, CGameBlackboard>
{
	public override string DisplayName => "BMSSV";
	public override string TypeName    => nameof(CGameBlackboard);

	public override FileVersion Version { get; } = new(3, 1, 0);

	/// <summary>
	/// A dictionary of all Blackboard sections present in the BMSSV file.
	/// </summary>
	public IDictionary<string, CBlackboard__CSection> Sections => Root.Sections;
}