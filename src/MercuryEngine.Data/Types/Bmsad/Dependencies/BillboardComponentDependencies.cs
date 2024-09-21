using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bmsad.Dependencies;

public class BillboardComponentDependencies : ComponentDependencies<BillboardComponentDependencies>
{
	public string Id1 { get; set; } = string.Empty;
	public string Id2 { get; set; } = string.Empty;

	public List<BillboardProp1> Unknown1 { get; } = [];
	public List<BillboardProp2> Unknown2 { get; } = [];

	protected override void Describe(DataStructureBuilder<BillboardComponentDependencies> builder)
		=> builder
			.Property(m => m.Id1)
			.Array(m => m.Unknown1)
			.Property(m => m.Id2)
			.Array(m => m.Unknown2);

	public sealed class BillboardProp1 : DataStructure<BillboardProp1>
	{
		public string Id { get; set; } = string.Empty;

		public uint Unknown1A { get; set; }
		public uint Unknown1B { get; set; }
		public uint Unknown1C { get; set; }

		public byte Unknown2 { get; set; }

		public uint Unknown3A { get; set; }
		public uint Unknown3B { get; set; }

		public float Unknown4 { get; set; }

		protected override void Describe(DataStructureBuilder<BillboardProp1> builder)
			=> builder
				.Property(m => m.Id)
				.Property(m => m.Unknown1A)
				.Property(m => m.Unknown1B)
				.Property(m => m.Unknown1C)
				.Property(m => m.Unknown2)
				.Property(m => m.Unknown3A)
				.Property(m => m.Unknown3B)
				.Property(m => m.Unknown4);
	}

	public sealed class BillboardProp2 : DataStructure<BillboardProp2>
	{
		public string Id { get; set; } = string.Empty;

		public byte Unknown1 { get; set; }

		public uint Unknown2A { get; set; }
		public uint Unknown2B { get; set; }
		public uint Unknown2C { get; set; }
		public uint Unknown2D { get; set; }

		protected override void Describe(DataStructureBuilder<BillboardProp2> builder)
			=> builder
				.Property(m => m.Id)
				.Property(m => m.Unknown1)
				.Property(m => m.Unknown2A)
				.Property(m => m.Unknown2B)
				.Property(m => m.Unknown2C)
				.Property(m => m.Unknown2D);
	}
}