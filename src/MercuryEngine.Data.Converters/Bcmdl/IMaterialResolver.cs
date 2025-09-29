using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.Converters.Bcmdl;

public interface IMaterialResolver
{
	Bsmat? LoadMaterial(string path);
	Bctex? LoadTexture(string path);
}