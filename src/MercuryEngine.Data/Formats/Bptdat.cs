using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bptdat : BaseStandardFormat<Bptdat, CPlaythrough>
{
	public override string DisplayName => "BPTDAT";

	[JsonIgnore] // The property on the structure itself should be serialized instead of this one
	public IDictionary<string, CPlaythrough__SCheckpointData?>? CheckpointData
	{
		get
		{
			if (Root.CheckpointDatas is null)
				return null;

			return new DictionaryAdapter<TerminatedStringField, DreadPointer<CPlaythrough__SCheckpointData>, string, CPlaythrough__SCheckpointData?>(
				Root.CheckpointDatas,
				bk => bk.Value,
				bv => bv.Value,
				ak => new TerminatedStringField(ak),
				av => new DreadPointer<CPlaythrough__SCheckpointData>(av)
			);
		}
	}
}