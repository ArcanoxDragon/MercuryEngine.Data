using System.Collections.ObjectModel;
using System.Linq.Expressions;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Bmsad.Dependencies;
using MercuryEngine.Data.Types.Fields;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bmsad;

public class ActorDefComponent : DataStructure<ActorDefComponent>
{
	#region Singleton Expressions

	private static readonly Expression<Func<ActorDefComponent, string?>>       TypeExpression        = m => m.Type;
	private static readonly Expression<Func<ActorDefComponent, int>>           Unknown1Expression    = m => m.Unknown1;
	private static readonly Expression<Func<ActorDefComponent, int>>           Unknown2Expression    = m => m.Unknown2;
	private static readonly Expression<Func<ActorDefComponent, FieldsWrapper>> InnerFieldsExpression = m => m.InnerFields;

	private static readonly Expression<Func<ActorDefComponent, ConditionalField<DictionaryField<TerminatedStringField, ExtraField>>>>
		RawExtraFieldsConditionExpression = m => m.RawExtraFieldsCondition;

	private static readonly Expression<Func<ActorDefComponent, IList<ComponentFunction>>> FunctionsExpression = m => m.Functions;

	private static readonly Expression<Func<ActorDefComponent, SwitchField<ComponentDependencies>>>
		RawDependenciesExpression = m => m.RawDependencies;

	#endregion

	private static readonly IDictionary<string, ExtraField> EmptyExtraFields
		= new ReadOnlyDictionary<string, ExtraField>(new Dictionary<string, ExtraField>());

	private readonly Dictionary<string, ExtraField> extraFields = [];

	private string type = "CComponent";

	public ActorDefComponent()
	{
		RawExtraFieldsCondition = new ConditionalField<DictionaryField<TerminatedStringField, ExtraField>>(
			() => DreadTypeLibrary.IsChildOf(Type, "CComponent"),
			RawExtraFields
		);
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

	private ConditionalField<DictionaryField<TerminatedStringField, ExtraField>> RawExtraFieldsCondition { get; }

	private DictionaryField<TerminatedStringField, ExtraField> RawExtraFields { get; }
		= DictionaryField.Create<TerminatedStringField, ExtraField>();

	private SwitchField<ComponentDependencies> RawDependencies { get; }

	protected override void Describe(DataStructureBuilder<ActorDefComponent> builder)
	{
		builder.Property(TypeExpression);
		builder.Property(Unknown1Expression);
		builder.Property(Unknown2Expression);
		builder.RawProperty(InnerFieldsExpression);
		builder.RawProperty(RawExtraFieldsConditionExpression);
		builder.Array(FunctionsExpression);
		builder.RawProperty(RawDependenciesExpression);
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

	private sealed class FieldsWrapper : IResettableField
	{
		public StrId             EmptyString { get; } = new("");
		public StrId             Root        { get; } = new("Root");
		public ITypedDreadField? Object      { get; set; }

		public uint Size      => sizeof(uint) + InnerSize;
		public uint InnerSize => Object is null ? 0 : ( EmptyString.Size + Root.Size + Object.Size );

		public bool HasMeaningfulData => Object != null;

		public void Reset()
		{
			Object = null;
		}

		public void Read(BinaryReader reader)
		{
			// Read length prefix. Stream must always advance this exact amount after the read.
			var length = reader.ReadUInt32();

			if (length == 0)
			{
				Reset();
				return;
			}

			var startPosition = reader.BaseStream.Position;

			EmptyString.Read(reader);
			Root.Read(reader);
			Object?.Read(reader);

			reader.BaseStream.Seek(startPosition + length, SeekOrigin.Begin);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(InnerSize);

			if (Object is null)
				return;

			EmptyString.Write(writer);
			Root.Write(writer);
			Object.Write(writer);
		}

		public async Task ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken = default)
		{
			// Read length prefix. Stream must always advance this exact amount after the read.
			var length = await reader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

			if (length == 0)
			{
				Reset();
				return;
			}

			var startPosition = reader.BaseStream.Position;

			await EmptyString.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
			await Root.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

			if (Object != null)
				await Object.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

			reader.BaseStream.Seek(startPosition + length, SeekOrigin.Begin);
		}

		public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken = default)
		{
			await writer.WriteAsync(InnerSize, cancellationToken).ConfigureAwait(false);

			if (Object is null)
				return;

			await EmptyString.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
			await Root.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
			await Object.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
		}
	}
}