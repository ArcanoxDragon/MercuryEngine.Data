using JetBrains.Annotations;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bptdef : BaseStandardFormat<Bptdef, CPlaythroughDef>
{
	public override string DisplayName => "BPTDEF";

	public IList<CPlaythroughDef__SCheckpointDef?>? CheckpointDefs
	{
		get
		{
			if (Root.CheckpointDefs is null)
				return null;

			return new ListAdapter<DreadPointer<CPlaythroughDef__SCheckpointDef>, CPlaythroughDef__SCheckpointDef?>(
				Root.CheckpointDefs,
				bv => bv.Value,
				av => new DreadPointer<CPlaythroughDef__SCheckpointDef>(av)
			);
		}
	}
}