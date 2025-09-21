using MercuryEngine.Data.Formats;

namespace MercuryEngine.Data.Converters.Bcmdl;

public interface IMaterialResolver
{
	Bsmat? LoadMaterial(string path);
	byte[]? LoadTexture(string path);
}