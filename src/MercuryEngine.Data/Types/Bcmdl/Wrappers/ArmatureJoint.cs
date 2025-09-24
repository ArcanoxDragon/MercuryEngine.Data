using System.ComponentModel;

namespace MercuryEngine.Data.Types.Bcmdl.Wrappers;

public class ArmatureJoint(string name)
{
	private readonly List<ArmatureJoint> children = [];

	public string Name { get; } = name;

	public Transform      Transform { get; set; } = new();
	public ArmatureJoint? Parent    { get; private set; }

	#region Unknown Data (in BCMDL)

	/// <summary>
	/// This represents a flag present on joints in a BCMDL file, the purpose of which is not known.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool UnknownFlag { get; set; }

	#endregion

	public bool                         IsRoot   => Parent is null;
	public IReadOnlyList<ArmatureJoint> Children => this.children;

	public void Add(ArmatureJoint child)
	{
		if (child.Parent != null)
			throw new ArgumentException("The child joint already has a parent");

		this.children.Add(child);
		child.Parent = this;
	}

	public bool Remove(ArmatureJoint child)
	{
		if (!this.children.Remove(child))
			return false;

		child.Parent = null;
		return true;
	}
}