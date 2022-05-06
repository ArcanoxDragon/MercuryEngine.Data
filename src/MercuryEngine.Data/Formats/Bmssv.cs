using JetBrains.Annotations;
using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmssv : BinaryFormat<Bmssv>
{
	public int  Unknown1     { get; private set; }
	public int  Unknown2     { get; private set; }
	public uint SectionCount { get; private set; }

	protected override void Describe(Builder builder)
		// TODO: Support CBlackboard too
		=> builder.CrcLiteral("CGameBlackboard")
				  .Int32(m => m.Unknown1)
				  .CrcLiteral("Root")
				  .Int32(m => m.Unknown2)
				  .CrcLiteral("hashSections")
				  .UInt32(m => m.SectionCount);
}

[PublicAPI]
public class BmssvSection : BinaryFormat<BmssvSection>
{
	public string Name            { get; set; } = string.Empty;
	public uint   DictionaryCount { get; private set; }

	protected override void Describe(Builder builder)
		=> builder.TerminatedString(m => m.Name)
				  .CrcLiteral("CBlackboard::CSection")
				  .UInt32(m => m.DictionaryCount);
}