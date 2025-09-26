using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;

namespace MercuryEngine.Data.Types.Bcmdl;

public class JointsInfo : DataStructure<JointsInfo>
{
	public JointsInfo()
	{
		JointFlagsField = CreateJointFlagsField();
	}

	public uint JointCount => (uint) ( JointsField?.Entries.Count ?? 0 );

	public IList<Joint?> Joints
	{
		get
		{
			JointsField ??= CreateJointsField();
			return JointsField.Entries;
		}
	}

	public IList<JointFlags?> JointFlags
	{
		get
		{
			JointFlagsField ??= CreateJointFlagsField();
			return JointFlagsField.Entries;
		}
	}

	#region Private Data

	private uint                         StoredJointCount { get; set; }
	private LinkedListField<Joint>?      JointsField      { get; set; } = CreateJointsField();
	private LinkedListField<JointFlags>? JointFlagsField  { get; set; }

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		StoredJointCount = JointCount;
	}

	#endregion

	private static LinkedListField<Joint> CreateJointsField()
		=> LinkedListField.Create<Joint>(startByteAlignment: 8);

	private LinkedListField<JointFlags> CreateJointFlagsField()
		=> new(() => new JointFlags(this), startByteAlignment: 8);

	protected override void Describe(DataStructureBuilder<JointsInfo> builder)
	{
		builder.Property(m => m.StoredJointCount);
		builder.Padding(4, 0xFF);
		builder.Pointer(m => m.JointsField, _ => CreateJointsField());
		builder.Pointer(m => m.JointFlagsField, owner => owner.CreateJointFlagsField(), endByteAlignment: 8);
	}
}