using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class VertexInfoDescription : DataStructure<VertexInfoDescription>
{
	public VertexInfoType Type        { get; set; }
	public uint           StartOffset { get; set; }
	public ushort         DataType    { get; set; } = 3; // TODO: ?
	public ushort         Count       { get; set; }
	public uint           Unknown     { get; set; }

	protected override void Describe(DataStructureBuilder<VertexInfoDescription> builder)
	{
		builder.Property(m => m.Type);
		builder.Property(m => m.StartOffset);
		builder.Property(m => m.DataType);
		builder.Property(m => m.Count);
		builder.Property(m => m.Unknown);
	}
}