using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

[PublicAPI]
public partial class CBlackboard
{
	private readonly DictionaryAdapter<TerminatedStringField, DreadPointer<CBlackboard__CSection>, string, CBlackboard__CSection> sectionsAdapter;

	public CBlackboard()
	{
		this.sectionsAdapter = new DictionaryAdapter<TerminatedStringField, DreadPointer<CBlackboard__CSection>, string, CBlackboard__CSection>(
			RawSections,
			bK => bK.Value,
			bV => bV.Value!,
			aK => new TerminatedStringField(aK),
			aV => new DreadPointer<CBlackboard__CSection>(aV)
		);
	}

	/// <summary>
	/// A dictionary of all Blackboard sections present in the BMSSV file.
	/// </summary>
	public IDictionary<string, CBlackboard__CSection> Sections => this.sectionsAdapter;

	[StructProperty("hashSections")]
	private IDictionary<TerminatedStringField, DreadPointer<CBlackboard__CSection>> RawSections
		=> RawFields.Dictionary<TerminatedStringField, DreadPointer<CBlackboard__CSection>>("hashSections");
}