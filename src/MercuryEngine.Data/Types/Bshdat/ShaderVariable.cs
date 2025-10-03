using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bshdat;

public class ShaderVariable : DataStructure<ShaderVariable>
{
	public const int StartAlignment = 0x08;

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

	public uint GlobalIndex { get; set; }

	#region Private Data

	private TerminatedStringField? NameField { get; set; }

	#endregion

	protected override void Describe(DataStructureBuilder<ShaderVariable> builder)
	{
		builder.Pointer(m => m.NameField);
		builder.Property(m => m.GlobalIndex);
	}
}