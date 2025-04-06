using JetBrains.Annotations;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmsss : BaseStandardFormat<Bmsss, DreadPointer<GUI__CGUIManager__SpriteSheetContainer>>
{
	public override string DisplayName => "BMSSS";
	public override string TypeName    => "GUI::CGUIManager::SpriteSheetContainer";

	public override FileVersion Version { get; } = new(1, 2, 2);

	/// <summary>
	/// A dictionary of all Sprite Sheets present in the BMSSS file.
	/// </summary>
	public IDictionary<string, GUI__CSpriteSheet> SpriteSheets
	{
		get
		{
			if (Root.Value is not { } container)
				throw new InvalidOperationException("Root container does not have a value!");

			return container.SpriteSheets;
		}
	}
}