using JetBrains.Annotations;
using MercuryEngine.Data.DataTypes;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework;
using MercuryEngine.Data.Framework.DataTypes.Structures;

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
				  .Int32(m => m.Unknown1)
				  .CrcLiteral("Root")
				  .Int32(m => m.Unknown2)
				  .CrcLiteral("hashSections")
				  .Array(m => m.Sections)
				  .CrcLiteral("dctDeltaValues")
				  .Int32(m => m.Unknown3);
}