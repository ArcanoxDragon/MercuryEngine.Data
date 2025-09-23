using MercuryEngine.Data.Formats;
using SkiaSharp;

namespace MercuryEngine.Data.Converters.Bcmdl;

public interface IMaterialResolver
{
	Bsmat? LoadMaterial(string path);
	SKBitmap? LoadTexture(string path);
}