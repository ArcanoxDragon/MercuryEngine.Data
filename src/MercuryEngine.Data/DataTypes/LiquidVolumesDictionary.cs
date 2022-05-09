using MercuryEngine.Data.Extensions;
using MercuryEngine.Data.Framework.DataTypes;
using MercuryEngine.Data.Framework.DataTypes.Structures;

namespace MercuryEngine.Data.DataTypes;

public class LiquidVolumesDictionary : DataStructure<LiquidVolumesDictionary>
{
	public Dictionary<TerminatedStringDataType, Entry> Entries { get; } = new();

	protected override void Describe(DataStructureBuilder<LiquidVolumesDictionary> builder)
		=> builder.Dictionary(m => m.Entries);

	public sealed class Entry : DataStructure<Entry>
	{
		// TODO: Proper properties map with ID/type lookup

		public Vector2 Min { get; set; } = new();
		public Vector2 Max { get; set; } = new();

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.Int32(2)
					  .CrcLiteral("Min")
					  .Structure(m => m.Min)
					  .CrcLiteral("Max")
					  .Structure(m => m.Max);
	}
}