using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad;

public class SoundFx : DataStructure<SoundFx>
{
	public string Name    { get; set; } = string.Empty;
	public byte   Unknown { get; set; }

	protected override void Describe(DataStructureBuilder<SoundFx> builder)
		=> builder
			.Property(m => m.Name)
			.Property(m => m.Unknown);
}