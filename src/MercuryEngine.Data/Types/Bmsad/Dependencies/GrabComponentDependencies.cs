using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

public class GrabComponentDependencies : ComponentDependencies<GrabComponentDependencies>
{
	public List<GrabDependency> Dependencies { get; } = [];

	protected override void Describe(DataStructureBuilder<GrabComponentDependencies> builder)
		=> builder.Array(m => m.Dependencies);

	public sealed class GrabDependency : DataStructure<GrabDependency>
	{
		public string Unknown1 { get; set; } = string.Empty;
		public string Unknown2 { get; set; } = string.Empty;
		public string Unknown3 { get; set; } = string.Empty;

		public float  Unknown4 { get; set; }
		public byte   Unknown5 { get; set; }
		public byte   Unknown6 { get; set; }
		public ushort Unknown7 { get; set; }

		public GrabValue Value1 { get; } = new();
		public GrabValue Value2 { get; } = new();

		protected override void Describe(DataStructureBuilder<GrabDependency> builder)
			=> builder
				.Property(m => m.Unknown1)
				.Property(m => m.Unknown2)
				.Property(m => m.Unknown3)
				.Property(m => m.Unknown4)
				.Property(m => m.Unknown5)
				.Property(m => m.Unknown6)
				.Property(m => m.Unknown7)
				.RawProperty(m => m.Value1)
				.RawProperty(m => m.Value2);
	}

	public sealed class GrabValue : DataStructure<GrabValue>
	{
		public ushort Unknown1 { get; set; }

		public float Value1 { get; set; }
		public float Value2 { get; set; }
		public float Value3 { get; set; }
		public float Value4 { get; set; }
		public float Value5 { get; set; }
		public float Value6 { get; set; }
		public float Value7 { get; set; }
		public float Value8 { get; set; }

		protected override void Describe(DataStructureBuilder<GrabValue> builder)
			=> builder
				.Property(m => m.Unknown1)
				.Property(m => m.Value1)
				.Property(m => m.Value2)
				.Property(m => m.Value3)
				.Property(m => m.Value4)
				.Property(m => m.Value5)
				.Property(m => m.Value6)
				.Property(m => m.Value7)
				.Property(m => m.Value8);
	}
}