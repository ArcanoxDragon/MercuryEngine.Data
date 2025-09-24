namespace MercuryEngine.Data.Types.Bcmdl;

public enum SkinningType
{
	Unknown,

	/// <summary>
	/// Joints use an inverse binding matrix derived solely from the mesh world matrix.
	/// </summary>
	WholeMeshTransform,

	/// <summary>
	/// Each joint uses a unique inverse binding matrix derived from the mesh world matrix and that joint's world matrix.
	/// </summary>
	PerJointTransform,
}