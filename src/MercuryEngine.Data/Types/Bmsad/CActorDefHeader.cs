using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework.Fields;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad;

[JsonDerivedType(typeof(CCharClassHeader))]
public class CActorDefHeader : DataStructure<CActorDefHeader>
{
	public bool Unknown1 { get; set; }
	public bool Unknown2 { get; set; }
	public int  Unknown3 { get; set; }
	public bool Unknown4 { get; set; }
	public bool Unknown5 { get; set; }

	// TODO: List adapter
	public List<TerminatedStringField> SubActors { get; } = [];

	protected override void Describe(DataStructureBuilder<CActorDefHeader> builder)
		=> builder
			.Property(m => m.Unknown1)
			.Property(m => m.Unknown2)
			.Property(m => m.Unknown3)
			.Property(m => m.Unknown4)
			.Property(m => m.Unknown5)
			.Array(m => m.SubActors);
}