using System.Diagnostics.CodeAnalysis;

namespace MercuryEngine.Data.Types.Bcmdl.Wrappers;

public class Armature
{
	#region Static API

	public static Armature FromBcmdl(Formats.Bcmdl source)
	{
		var armature = new Armature();

		armature.PopulateFromBcmdl(source);

		return armature;
	}

	#endregion

	private readonly List<ArmatureJoint>               allJoints    = [];
	private readonly Dictionary<string, ArmatureJoint> jointsByName = [];

	public ArmatureJoint this[int index]
	{
		get
		{
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.allJoints.Count);

			return this.allJoints[index];
		}
	}

	public ArmatureJoint? this[string name]
		=> this.jointsByName.GetValueOrDefault(name);

	public IReadOnlyList<ArmatureJoint> AllJoints => this.allJoints;

	public IEnumerable<ArmatureJoint> RootJoints => this.allJoints.Where(j => j.IsRoot);

	public bool TryGetJoint(int index, [NotNullWhen(true)] out ArmatureJoint? joint)
	{
		if (index < 0 || index >= this.allJoints.Count)
		{
			joint = null;
			return false;
		}

		joint = this.allJoints[index];
		return true;
	}

	private void PopulateFromBcmdl(Formats.Bcmdl source)
	{
		var waitingForParent = new Dictionary<string, List<ArmatureJoint>>();

		this.allJoints.Clear();
		this.jointsByName.Clear();

		if (source.JointsInfo is not { } jointsInfo)
			return;

		var index = 0;

		foreach (var joint in jointsInfo.Joints)
		{
			if (joint is null)
			{
				// This should never happen, but to handle it correctly in case it does, we just add a dummy joint with no parent or children
				AddJoint(new ArmatureJoint($"Dummy_{index:D3}"));
				continue;
			}

			var jointName = joint.Name ?? $"Unnamed_{index:D3}";
			var newJoint = new ArmatureJoint(jointName);

			joint.Transform?.CopyTo(newJoint.Transform);

			// Populate joint's children that we found before it
			if (waitingForParent.Remove(jointName, out var children))
			{
				foreach (var child in children)
					newJoint.Add(child);
			}

			if (joint.ParentName != null)
			{
				// Add this joint to its parent, if that parent has been found already. If not, add it to the queue.

				if (this.jointsByName.TryGetValue(joint.ParentName, out var parentJoint))
					parentJoint.Add(newJoint);
				else
					EnqueueForParent(joint.ParentName, newJoint);
			}

			AddJoint(newJoint);
			index++;
		}

		if (waitingForParent.Count > 0)
		{
			// If any joints were still waiting to be linked with a parent once we got done with the list,
			// we should throw an exception. This means a joint referenced another joint that did not exist.
			var jointNamesWaitingForParent = waitingForParent.Values.SelectMany(list => list).Select(joint => joint.Name);

			throw new ApplicationException($"The following joints referenced invalid parent joints: {string.Join(", ", jointNamesWaitingForParent)}");
		}

		return;

		void EnqueueForParent(string parentName, ArmatureJoint joint)
		{
			if (!waitingForParent.TryGetValue(parentName, out var joints))
			{
				joints = [];
				waitingForParent[parentName] = joints;
			}

			joints.Add(joint);
		}

		void AddJoint(ArmatureJoint joint)
		{
			this.allJoints.Add(joint);
			this.jointsByName[joint.Name] = joint;
		}
	}
}