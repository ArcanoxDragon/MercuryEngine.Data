using JetBrains.Annotations;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

[PublicAPI]
public partial class GUI__CGUIManager__SpriteSheetContainer
{
	private readonly DictionaryAdapter<TerminatedStringField, DreadPointer<GUI__CSpriteSheet>, string, GUI__CSpriteSheet> sectionsAdapter;

	public GUI__CGUIManager__SpriteSheetContainer()
	{
		this.sectionsAdapter = new DictionaryAdapter<TerminatedStringField, DreadPointer<GUI__CSpriteSheet>, string, GUI__CSpriteSheet>(
			RawSpriteSheets,
			bK => bK.Value,
			bV => bV.Value!,
			aK => new TerminatedStringField(aK),
			aV => new DreadPointer<GUI__CSpriteSheet>(aV)
		);
	}

	/// <summary>
	/// A dictionary of all Sprite Sheets present in the BMSSS file.
	/// </summary>
	public IDictionary<string, GUI__CSpriteSheet> SpriteSheets => this.sectionsAdapter;

	[StructProperty("mapSpriteSheets")]
	private IDictionary<TerminatedStringField, DreadPointer<GUI__CSpriteSheet>> RawSpriteSheets
		=> RawFields.Dictionary<TerminatedStringField, DreadPointer<GUI__CSpriteSheet>>("mapSpriteSheets");
}