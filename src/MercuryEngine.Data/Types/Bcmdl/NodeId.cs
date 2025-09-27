using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Definitions.Utility;

namespace MercuryEngine.Data.Types.Bcmdl;

public class NodeId : DataStructure<NodeId>
{
	public NodeId() { }

	public NodeId(string name, bool visible = true)
	{
		Name = name;
		Visible = visible;
	}

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

	public bool Visible { get; set; } = true;

	#region Private Data

	private TerminatedStringField? NameField { get; set; }

	#endregion

	#region Hooks

	protected override void AfterRead(ReadContext context)
	{
		base.AfterRead(context);

		KnownStrings.Record(Name);
	}

	#endregion

	protected override void Describe(DataStructureBuilder<NodeId> builder)
	{
		builder.Pointer(m => m.NameField, unique: true);
		builder.Property(m => m.Visible);

		// 7 bytes of padding
		builder.Padding(7, 0xFF);
	}
}