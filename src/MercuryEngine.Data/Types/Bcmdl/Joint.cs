using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class Joint : DataStructure<Joint>
{
	public Transform? Transform { get; set; }

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

	public string? ParentName
	{
		get => ParentNameField?.Value;
		set
		{
			if (value is null)
				ParentNameField = null;
			else
				( ParentNameField ??= new TerminatedStringField() ).Value = value;
		}
	}

	public bool UnkFlag { get; set; }

	#region Private Data

	private TerminatedStringField? NameField       { get; set; }
	private TerminatedStringField? ParentNameField { get; set; }

	#endregion

	protected override void Describe(DataStructureBuilder<Joint> builder)
	{
		builder.Pointer(m => m.Transform);
		builder.Pointer(m => m.NameField, unique: true);
		builder.Pointer(m => m.ParentNameField, unique: true);
		builder.Property(m => m.UnkFlag);
	}
}