using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class SubmeshInfo : DataStructure<SubmeshInfo>
{
	public uint SkinningType       { get; set; }
	public uint IndexOffset        { get; set; }
	public uint IndexCount         { get; set; }
	public uint JointMapEntryCount { get; set; }

	public uint[] JointMap
	{
		get
		{
			JointMapField ??= CreateJointMapField();
			return JointMapField.Entries;
		}
	}

	#region Private Data

	private JointMap? JointMapField { get; set; }

	#endregion

	private JointMap CreateJointMapField()
		=> new(this);

	protected override void Describe(DataStructureBuilder<SubmeshInfo> builder)
	{
		builder.Property(m => m.SkinningType);
		builder.Property(m => m.IndexOffset);
		builder.Property(m => m.IndexCount);
		builder.Property(m => m.JointMapEntryCount);
		builder.Pointer(m => m.JointMapField, owner => owner.CreateJointMapField(), startByteAlignment: 8);
	}
}