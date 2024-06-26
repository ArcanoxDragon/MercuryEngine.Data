using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Extensions;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmssv : BinaryFormat<Bmssv>, IDescribeDataStructure<Bmssv>
{
	private readonly DictionaryAdapter<TerminatedStringField, DreadTypePrefixedField, string, CBlackboard__CSection> sectionsAdapter;

	public Bmssv()
	{
		this.sectionsAdapter = new DictionaryAdapter<TerminatedStringField, DreadTypePrefixedField, string, CBlackboard__CSection>(
			RawSections,
			bK => bK.Value,
			bV => (CBlackboard__CSection) bV.InnerData!,
			aK => new TerminatedStringField(aK),
			aV => new DreadTypePrefixedField(aV)
		);
	}

	public override string DisplayName => "BMSSV";

	/// <summary>
	/// A dictionary of all Blackboard sections present in the BMSSV file.
	/// </summary>
	public IDictionary<string, CBlackboard__CSection> Sections => this.sectionsAdapter;

	private short Unknown1                { get; set; }
	private short Unknown2                { get; set; }
	private int   RootPlaceholder         { get; set; }
	private int   DeltaValues_Placeholder { get; set; }

	private Dictionary<TerminatedStringField, DreadTypePrefixedField> RawSections { get; set; } = [];

	public static void Describe(DataStructureBuilder<Bmssv> builder)
		// TODO: Support CBlackboard too
		=> builder.CrcConstant("CGameBlackboard")
			.Property(m => m.Unknown1)
			.Property(m => m.Unknown2)
			.CrcConstant("Root")
			.Property(m => m.RootPlaceholder)
			.CrcConstant("hashSections")
			.Dictionary(m => m.RawSections)
			.CrcConstant("dctDeltaValues")
			.Property(m => m.DeltaValues_Placeholder);
}