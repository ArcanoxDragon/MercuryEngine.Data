using MercuryEngine.Data.Core.Framework.DataTypes;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Extensions;

namespace MercuryEngine.Data.Types.DataTypes.Custom;

public class LiquidVolumesDictionary : DataStructure<LiquidVolumesDictionary>, IDreadDataType
{
	public string TypeName => "LiquidVolumesDictionary";

	public Dictionary<TerminatedStringDataType, Entry> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<LiquidVolumesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		public Vector2? Min { get; set; }
		public Vector2? Max { get; set; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.MsePropertyBag(fields => {
				fields.Structure("Min", m => m.Min)
					  .Structure("Max", m => m.Max);
			});
	}
}