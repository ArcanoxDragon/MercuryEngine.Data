using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.DreadTypes;

namespace MercuryEngine.Data.Types.Bmsad;

public class CCharClassHeader : CActorDefHeader
{
	public string ModelName { get; set; } = string.Empty;

	public float   Unknown6  { get; set; }
	public float   Unknown7  { get; set; }
	public float   Unknown8  { get; set; }
	public float   Unknown9  { get; set; }
	public float   Unknown10 { get; set; }
	public Vector3 Unknown11 { get; set; } = new();
	public float   Unknown12 { get; set; }
	public bool    Unknown13 { get; set; }

	public string Category { get; set; } = string.Empty;

	protected override void Describe(DataStructureBuilder<CActorDefHeader> builder)
	{
		builder
			.For<CCharClassHeader>()
			.Property(m => m.ModelName);

		base.Describe(builder);

		builder
			.For<CCharClassHeader>()
			.Property(m => m.Unknown6)
			.Property(m => m.Unknown7)
			.Property(m => m.Unknown8)
			.Property(m => m.Unknown9)
			.Property(m => m.Unknown10)
			.RawProperty(m => m.Unknown11)
			.Property(m => m.Unknown12)
			.Constant(0xFFFFFFFF, "<magic>")
			.Property(m => m.Unknown13)
			.Property(m => m.Category);
	}
}