using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
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
		public Entry()
		{
			RawFields = MsePropertyBagField.Create(fields => {
				fields.AddField<Vector2>(nameof(Min));
				fields.AddField<Vector2>(nameof(Max));
			});
		}

		public Vector2? Min
		{
			get => RawFields.Get<Vector2>(nameof(Min));
			set => RawFields.SetOrClear(nameof(Min), value);
		}

		public Vector2? Max
		{
			get => RawFields.Get<Vector2>(nameof(Max));
			set => RawFields.SetOrClear(nameof(Max), value);
		}

		public MsePropertyBagField RawFields { get; }

		protected override void Describe(DataStructureBuilder<Entry> builder)
			=> builder.RawProperty(m => m.RawFields);
	}
}