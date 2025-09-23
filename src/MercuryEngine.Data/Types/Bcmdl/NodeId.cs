using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class NodeId : DataStructure<NodeId>
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

	public bool Visible { get; set; }

	#region Private Data

	private TerminatedStringField? NameField { get; set; }

	#endregion

	protected override void Describe(DataStructureBuilder<NodeId> builder)
	{
		builder.Pointer(m => m.NameField, unique: true);
		builder.Property(m => m.Visible);

		// 7 bytes of padding
		builder.Padding(7, 0xFF);
	}
}