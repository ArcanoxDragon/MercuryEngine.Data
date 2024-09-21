using JetBrains.Annotations;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Core.Utility;
using MercuryEngine.Data.Types.Bmsad;

namespace MercuryEngine.Data.Formats;

[PublicAPI]
public class Bmsad : BinaryFormat<Bmsad>
{
	private const string CharClassHeaderType = "CCharClass";
	private const string ActorDefHeaderType  = "CActorDef";

	public Bmsad()
	{
		RawHeader = SwitchField<CActorDefHeader>.FromProperty(this, m => m.Type, builder => {
			builder.AddCase(CharClassHeaderType, new CCharClassHeader());
			builder.AddCase(ActorDefHeaderType, new CActorDefHeader());
			builder.AddFallback(new CActorDefHeader());
		});

		Components = new DictionaryAdapter<TerminatedStringField, ActorDefComponent, string, ActorDefComponent>(
			RawComponents,
			bk => bk.Value,
			bv => bv,
			ak => new TerminatedStringField(ak),
			av => av
		);
	}

	public override string DisplayName => "BMSAD";

	public string Name    { get; set; }         = string.Empty;
	public string Type    { get; private set; } = string.Empty;
	public bool   Unknown { get; set; }

	public CActorDefHeader Header
	{
		get => RawHeader.EffectiveField;
		set
		{
			// Set Type first so the correct switch case field gets assigned
			Type = value switch {
				CCharClassHeader => CharClassHeaderType,
				not null         => ActorDefHeaderType,
				_                => throw new ArgumentNullException(nameof(value)),
			};
			RawHeader.EffectiveField = value;
		}
	}

	public IDictionary<string, ActorDefComponent> Components { get; }

	private Dictionary<TerminatedStringField, ActorDefComponent> RawComponents { get; } = [];

	// TODO: List adapter
	public List<TerminatedStringField> ActionSets { get; } = [];

	public List<SoundFx> SoundFx { get; } = [];

	private SwitchField<CActorDefHeader> RawHeader { get; }

	protected override void Describe(DataStructureBuilder<Bmsad> builder)
		=> builder
			.Constant("MSAD", "<magic>", terminated: false)
			.Constant(0x0200000F, "version")
			.Property(m => m.Name)
			.Property(m => m.Type)
			.RawProperty(m => m.RawHeader)
			.Property(m => m.Unknown)
			.Dictionary(m => m.RawComponents)
			.Array(m => m.ActionSets)
			.Array(m => m.SoundFx);
}