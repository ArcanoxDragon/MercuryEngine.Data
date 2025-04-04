using JetBrains.Annotations;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmscp : BaseStandardFormat<Bmscp, DreadPointer<GUI__CDisplayObjectContainer>>
{
	public override string DisplayName => "BMSCP";
	public override string TypeName    => "GUI::CDisplayObjectContainer";

	public override FileVersion Version { get; } = new(1, 2, 2);

	public IList<GUI__CDisplayObject?> Children
	{
		get
		{
			if (Root.Value is not { } container)
				throw new InvalidOperationException("Root container does not have a value!");

			return container.Children;
		}
	}
}