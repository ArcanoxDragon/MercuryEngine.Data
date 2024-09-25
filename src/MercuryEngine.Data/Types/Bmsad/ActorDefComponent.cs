using System.Collections.ObjectModel;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bmsad.Dependencies;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Bmsad;

public class ActorDefComponent : DataStructure<ActorDefComponent>
{
	private static readonly IDictionary<string, ExtraField> EmptyExtraFields
		= new ReadOnlyDictionary<string, ExtraField>(new Dictionary<string, ExtraField>());

	private readonly Dictionary<string, ExtraField> extraFields = [];

	private string type = "CComponent";

	public ActorDefComponent()
	{
		RawDependencies = ComponentDependencies.Create(this);
	}

	public string Type // Aliased so that we can make it publicly init-only, but privately settable
	{
		get => RawType;
		init => RawType = value;
	}

	public new ITypedDreadField? Fields => InnerFields.Object;

	private string RawType
	{
		get => this.type;
		set
		{
			this.type = value;

			// When setting the Type (such as during reading), we also set the object on InnerFields to an empty instance of the appropriate CharClass.
			var charClassTypeName = GetCharClassTypeName(Type);

			InnerFields.Object = DreadTypeLibrary.GetTypedField(charClassTypeName);
		}
	}

	public int Unknown1 { get; set; }
	public int Unknown2 { get; set; }

	public List<ComponentFunction> Functions { get; } = [];

	public IDictionary<string, ExtraField> ExtraFields
	{
		get
		{
			if (!DreadTypeLibrary.IsChildOf(Type, "CComponent"))
				return EmptyExtraFields;

			return this.extraFields;
		}
	}

	public ComponentDependencies Dependencies => RawDependencies.EffectiveField;

	internal string TypeForDependencies
		=> ComponentDependencies.KnownComponentDependencyTypes.FirstOrDefault(
			type => DreadTypeLibrary.IsChildOf(Type, type), "CComponent");

	/// <summary>Wrapper around the length-prefixed sub-structure</summary>
	private FieldsWrapper InnerFields { get; } = new();

	private DictionaryField<TerminatedStringField, ExtraField> RawExtraFields { get; } = DictionaryField.Create<TerminatedStringField, ExtraField>();

	private SwitchField<ComponentDependencies> RawDependencies { get; }

	protected override void Describe(DataStructureBuilder<ActorDefComponent> builder)
	{
		builder
			.Property(m => m.Type)
			.Property(m => m.Unknown1)
			.Property(m => m.Unknown2)
			.RawField(new LengthPrefixedField<FieldsWrapper>(InnerFields, validateReads: false), $"{nameof(Fields)}: LengthPrefixed")
			.RawField(
				new ConditionalField<DictionaryField<TerminatedStringField, ExtraField>>(
					() => DreadTypeLibrary.IsChildOf(Type, "CComponent"),
					RawExtraFields
				),
				$"{nameof(ExtraFields)}: Dictionary<string, {nameof(ExtraField)}>?"
			)
			.Array(m => m.Functions)
			.RawProperty(m => m.RawDependencies);
	}

	protected override void BeforeWrite()
	{
		// Manually transfer ExtraFields to RawExtraFields

		RawExtraFields.Value.Clear();

		foreach (var (key, value) in this.extraFields)
			RawExtraFields.Value.Add(new TerminatedStringField(key), value);
	}

	protected override void AfterRead()
	{
		// Manually transfer RawExtraFields to ExtraFields

		this.extraFields.Clear();

		foreach (var (key, value) in RawExtraFields.Value)
			this.extraFields[key.Value] = value;
	}

	private static string GetCharClassTypeName(string actorDefTypeName)
	{
		if (actorDefTypeName == "CActorComponent")
			return "CActorComponentDef";

		var classNameMinusSuffix = actorDefTypeName.Length > 1 ? actorDefTypeName[1..] : actorDefTypeName;
		var charClassName = $"CCharClass{classNameMinusSuffix}";

		if (DreadTypeLibrary.TryFindType(charClassName, out var charClassType))
			return charClassType.TypeName;

		if (DreadTypeLibrary.GetParent(actorDefTypeName) is { } parentTypeName)
			return GetCharClassTypeName(parentTypeName);

		throw new NotSupportedException($"Unable to determine character class type name for \"{actorDefTypeName}\"");
	}

	private sealed class FieldsWrapper : DataStructure<FieldsWrapper>
	{
		public StrId             EmptyString { get; } = new();
		public StrId             Root        { get; } = new();
		public ITypedDreadField? Object      { get; set; }

		public override bool HasMeaningfulData => Object != null;

		public override void Reset()
		{
			base.Reset();
			Object = null;
		}

		protected override void Describe(DataStructureBuilder<FieldsWrapper> builder)
			=> builder
				.RawProperty(m => m.EmptyString)
				.RawProperty(m => m.Root)
				.NullableRawProperty(m => m.Object, () => new base__core__CBaseObject());
	}
}