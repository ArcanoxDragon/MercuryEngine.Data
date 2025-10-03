namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

[Flags]
public enum ShaderStageFlags
{
	None,
	Vertex   = 1 << 0,
	Fragment = 1 << 1,
	Geometry = 1 << 2,
}