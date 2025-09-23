using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class MeshNode : DataStructure<MeshNode>
{
	public Mesh?     Mesh     { get; set; }
	public Material? Material { get; set; }
	public NodeId?   Id       { get; set; }
	public bool      Visible  { get; set; }

	protected override void Describe(DataStructureBuilder<MeshNode> builder)
	{
		builder.Pointer(m => m.Mesh);
		builder.Pointer(m => m.Material);
		builder.Pointer(m => m.Id);
		builder.Property(m => m.Visible);

		// 7 bytes of padding, the first 3 bytes of which are all 0xFF, and the rest are 0x00
		builder.Padding(3, 0xFF);
		builder.Padding(4);
	}
}