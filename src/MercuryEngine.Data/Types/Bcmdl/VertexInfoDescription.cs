using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class VertexInfoDescription : DataStructure<VertexInfoDescription>
{
	public VertexInfoDescription() { }

	public VertexInfoDescription(VertexInfoType type)
	{
		Type = type;
		Count = type switch {
			VertexInfoType.Position    => 3,
			VertexInfoType.Normal      => 3,
			VertexInfoType.Color       => 4,
			VertexInfoType.UV1         => 2,
			VertexInfoType.UV2         => 2,
			VertexInfoType.UV3         => 2,
			VertexInfoType.Tangent     => 4,
			VertexInfoType.JointIndex  => 4,
			VertexInfoType.JointWeight => 4,

			_ => throw new ArgumentException($"Unrecognized {nameof(VertexInfoType)}: {type}", nameof(type)),
		};
	}

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