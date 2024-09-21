using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.Bmsad;

public class ExtraField : DataStructure<ExtraField>
{
	public ExtraField()
	{
		Value = false;

		ValueField = SwitchField.FromProperty(this, m => m.Type, builder => {
			builder.AddCase("bool", BooleanField);
			builder.AddCase("string", StringField);
			builder.AddCase("float", FloatField);
			builder.AddCase("int", IntField);
			builder.AddCase("vec3", Vector3Field);
		});
	}

	public string Type { get; private set; }

	public object Value
	{
		get => Type switch {
			"bool"   => BooleanField.Value,
			"string" => StringField.Value,
			"float"  => FloatField.Value,
			"int"    => IntField.Value,
			"vec3"   => Vector3Field,
			_        => throw new InvalidOperationException($"Field has invalid type \"{Type}\""),
		};

		[MemberNotNull(nameof(Type))]
		set
		{
			ArgumentNullException.ThrowIfNull(value);

			object normalized = NormalizeValueType(value);

			Type = GetValueTypeName(normalized);

			switch (normalized)
			{
				case bool b:
					BooleanField.Value = b;
					break;
				case string s:
					StringField.Value = s;
					break;
				case float f:
					FloatField.Value = f;
					break;
				case int i:
					IntField.Value = i;
					break;
				case Vector3 v:
					Vector3Field = v;
					break;
			}
		}
	}

	private SwitchField<IBinaryField> ValueField { get; }

	#region Typed Value Fields

	private BooleanField          BooleanField { get; }      = new();
	private TerminatedStringField StringField  { get; }      = new();
	private FloatField            FloatField   { get; }      = new();
	private Int32Field            IntField     { get; }      = new();
	private Vector3               Vector3Field { get; set; } = new();

	#endregion

	protected override void Describe(DataStructureBuilder<ExtraField> builder)
		=> builder
			.Property(m => m.Type)
			.RawProperty(m => m.ValueField);

	private static string GetValueTypeName(object value)
		=> value switch {
			bool    => "bool",
			string  => "string",
			float   => "float",
			int     => "int",
			Vector3 => "vec3",

			_ => throw new ArgumentException($"Unsupported value type: {value.GetType().FullName}", nameof(value)),
		};

	private static object NormalizeValueType(object value)
		=> value switch {
			bool    => value,
			string  => value,
			float   => value,
			int     => value,
			Vector3 => value,

			decimal d => (float) d,
			double d  => (float) d,
			byte b    => (int) b,
			sbyte b   => (int) b,
			uint i    => (int) i,
			long l    => (int) l,
			ulong l   => (int) l,

			_ => throw new ArgumentException($"Unsupported value type: {value.GetType().FullName}", nameof(value)),
		};
}