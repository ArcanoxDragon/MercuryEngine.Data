namespace MercuryEngine.Data.Types.Bcmdl;

public enum SkinningType
{
	Unknown,

	/// <summary>
	/// Mesh is rigged "rigidly" to a single parent bone. Weights are not used, and the joint bind matrix is assumed to
	/// be the identity matrix (the mesh is positioned using its translation, and then affected by the bone).
	/// </summary>
	Rigid,

	/// <summary>
	/// Mesh is rigged "softly" to one or more bones. Vertex weights control bone influence. The mesh is positioned at
	/// the model origin, and the inverse bind matrices of the joints are used to correctly position it in space.
	/// </summary>
	Soft,
}