using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmssk : BaseStandardFormat<Bmssk, DreadPointer<GUI__CGUIManager__SkinContainer>>
{
	public override string DisplayName => "BMSSK";
	public override string TypeName    => "GUI::CGUIManager::SkinContainer";

	public override FileVersion Version { get; } = new(1, 2, 2);

	[JsonIgnore]
	public IList<GUI__CSkin?> Skins
	{
		get
		{
			if (Root.Value is not { } container)
				throw new InvalidOperationException("Root container does not have a value!");

			return container.Skins;
		}
	}
}