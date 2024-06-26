using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Extensions;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.DreadTypes.Custom;

public class LiquidVolumesDictionary : DataStructure<LiquidVolumesDictionary>, ITypedDreadField
{
	public string TypeName => "LiquidVolumesDictionary";

	public Dictionary<TerminatedStringField, Entry> Entries { get; } = [];

	protected override void Describe(DataStructureBuilder<LiquidVolumesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public Vector2? Min { get; set; }
		public Vector2? Max { get; set; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.MsePropertyBag(fields => {
				fields.RawProperty("Min", m => m.Min)
					.RawProperty("Max", m => m.Max);
			});
	}
}