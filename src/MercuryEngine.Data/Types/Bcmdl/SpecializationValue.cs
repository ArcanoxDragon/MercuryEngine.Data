using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class SpecializationValue : DataStructure<SpecializationValue>
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

	public float Value { get; set; }

	#region Private Data

	private TerminatedStringField? NameField { get; set; }

	#endregion

	protected override void Describe(DataStructureBuilder<SpecializationValue> builder)
	{
		builder.Pointer(m => m.NameField, unique: true);
		builder.Property(m => m.Value);
		builder.Padding(4, 0xFF);
	}
}