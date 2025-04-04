using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Attributes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes;

public partial class GUI__CDisplayObjectContainer
{
	/// <summary>
	/// Field: lstChildren&#10;Original type: base::global::CPooledList&lt;GUI::CDisplayObject*&gt;
	/// </summary>
	[StructProperty("lstChildren")]
	public IList<DreadPointer<GUI__CDisplayObject>> RawChildren
		=> RawFields.Array<DreadPointer<GUI__CDisplayObject>>("lstChildren");

	[field: MaybeNull]
	public IList<GUI__CDisplayObject?> Children
		=> field ??= new ListAdapter<DreadPointer<GUI__CDisplayObject>, GUI__CDisplayObject?>(
			RawChildren,
			bv => bv.Value,
			av => new DreadPointer<GUI__CDisplayObject>(av)
		);
}