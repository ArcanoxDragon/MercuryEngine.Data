using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Definitions.DataTypes;
using MercuryEngine.Data.Definitions.Extensions;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmssv : BinaryFormat<Bmssv>
{
	public override string DisplayName => "BMSSV";

	public int Unknown1 { get; private set; }
	public int Unknown2 { get; private set; }
	public int Unknown3 { get; private set; }

	public List<CBlackboard_CSection> Sections { get; } = new();

	protected override void Describe(DataStructureBuilder<Bmssv> builder)
		// TODO: Support CBlackboard too
		=> builder.CrcLiteral("CGameBlackboard")
				  .Property(m => m.Unknown1)
				  .CrcLiteral("Root")
				  .Property(m => m.Unknown2)
				  .CrcLiteral("hashSections")
				  .Array(m => m.Sections)
				  .CrcLiteral("dctDeltaValues")
				  .Property(m => m.Unknown3);
}