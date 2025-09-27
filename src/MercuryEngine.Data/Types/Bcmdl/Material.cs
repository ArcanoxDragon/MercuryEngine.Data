using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Definitions.Utility;

namespace MercuryEngine.Data.Types.Bcmdl;

public class Material : DataStructure<Material>
{
	public string? Name
	{
		get => NameField?.Value;
		set
		{
			if (value is null)
				NameField = null;
			else
				( NameField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public string? Path
	{
		get => PathField?.Value;
		set
		{
			if (value is null)
				PathField = null;
			else
				( PathField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public string? Prefix
	{
		get => PrefixField?.Value;
		set
		{
			if (value is null)
				PrefixField = null;
			else
				( PrefixField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public MaterialParameters Parameters { get; } = new();

	public string? Tex1Name
	{
		get => Tex1NameField?.Value;
		set
		{
			if (value is null)
				Tex1NameField = null;
			else
				( Tex1NameField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public TextureParameters? Tex1Parameters { get; set; }

	public string? Tex2Name
	{
		get => Tex2NameField?.Value;
		set
		{
			if (value is null)
				Tex2NameField = null;
			else
				( Tex2NameField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public TextureParameters? Tex2Parameters { get; set; }

	public string? Tex3Name
	{
		get => Tex3NameField?.Value;
		set
		{
			if (value is null)
				Tex3NameField = null;
			else
				( Tex3NameField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public TextureParameters? Tex3Parameters { get; set; }

	public MaterialFlags Flags { get; } = new();

	#region Private Fields

	private TerminatedStringField? NameField     { get; set; }
	private TerminatedStringField? PathField     { get; set; }
	private TerminatedStringField? PrefixField   { get; set; }
	private TerminatedStringField? Tex1NameField { get; set; }
	private TerminatedStringField? Tex2NameField { get; set; }
	private TerminatedStringField? Tex3NameField { get; set; }

	#endregion

	#region Hooks

	protected override void AfterRead(ReadContext context)
	{
		base.AfterRead(context);

		KnownStrings.Record(Name);
		KnownStrings.Record(Path);
		KnownStrings.Record(Prefix);
		KnownStrings.Record(Tex1Name);
		KnownStrings.Record(Tex2Name);
		KnownStrings.Record(Tex3Name);
	}

	#endregion

	protected override void Describe(DataStructureBuilder<Material> builder)
	{
		builder.Pointer(m => m.NameField, unique: true);
		builder.Pointer(m => m.PathField, unique: true);
		builder.Pointer(m => m.PrefixField, unique: true);
		builder.RawProperty(m => m.Parameters);
		builder.Pointer(m => m.Tex1NameField, unique: true);
		builder.Pointer(m => m.Tex1Parameters, startByteAlignment: 8, endByteAlignment: 8);
		builder.Pointer(m => m.Tex2NameField, unique: true);
		builder.Pointer(m => m.Tex2Parameters, startByteAlignment: 8, endByteAlignment: 8);
		builder.Pointer(m => m.Tex3NameField, unique: true);
		builder.Pointer(m => m.Tex3Parameters, startByteAlignment: 8, endByteAlignment: 8);
		builder.RawProperty(m => m.Flags);
	}
}