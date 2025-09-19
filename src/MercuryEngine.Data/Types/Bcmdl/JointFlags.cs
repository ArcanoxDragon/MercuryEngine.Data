using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Fields;

namespace MercuryEngine.Data.Types.Bcmdl;

public class JointFlags(JointsInfo parent) : DataStructure<JointFlags>
{
	private bool[] flags = new bool[parent.JointCount];

	public StrId RootName { get; set; } = new("Top");

	public bool[] Flags
	{
		get
		{
			ResizeArrayIfNeeded();
			return this.flags;
		}
	}

	#region Private Data

	private RawBytes RawFlags { get; } = new(() => (int) parent.JointCount);

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		// Ensure backing field is the correct size
		if (RawFlags.Value.Length != this.flags.Length)
			RawFlags.Value = new byte[this.flags.Length];

		// Copy all boolean flags to the backing field
		Buffer.BlockCopy(this.flags, 0, RawFlags.Value, 0, this.flags.Length);
	}

	protected override void AfterRead(ReadContext context)
	{
		base.AfterRead(context);

		// Ensure flags array is the correct size
		if (this.flags.Length != RawFlags.Value.Length)
			this.flags = new bool[RawFlags.Value.Length];

		// Copy all flags from the backing field to our boolean array
		Buffer.BlockCopy(RawFlags.Value, 0, this.flags, 0, this.flags.Length);
	}

	#endregion

	private void ResizeArrayIfNeeded()
	{
		if (this.flags.Length == parent.JointCount)
			return;

		bool[] newArray = new bool[parent.JointCount];

		Array.Copy(this.flags, newArray, Math.Min(this.flags.Length, newArray.Length));
		this.flags = newArray;
	}

	protected override void Describe(DataStructureBuilder<JointFlags> builder)
	{
		builder.RawProperty(m => m.RootName);
		builder.RawProperty(m => m.RawFlags);
	}
}