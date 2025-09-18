using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

public partial class GUI__CGUIManager__SkinContainer
{
	/// <summary>
	/// Field: vecSkins&#10;Original type: base::global::CRntVector&lt;GUI::CSkin*&gt;
	/// </summary>
	[StructProperty("vecSkins")]
	public IList<DreadPointer<GUI__CSkin>> RawSkins
		=> RawFields.Array<DreadPointer<GUI__CSkin>>("vecSkins");

	[field: MaybeNull]
	[JsonIgnore]
	public IList<GUI__CSkin?> Skins
		=> field ??= new ListAdapter<DreadPointer<GUI__CSkin>, GUI__CSkin?>(
			RawSkins,
			bv => bv.Value,
			av => new DreadPointer<GUI__CSkin>(av)
		);
}