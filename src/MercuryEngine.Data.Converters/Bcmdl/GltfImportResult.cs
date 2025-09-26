using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib;

namespace MercuryEngine.Data.Converters.Bcmdl;

public class GltfImportResult(Formats.Bcmdl model)
{
	public Formats.Bcmdl Model { get; } = model;

	public List<Bsmat> Materials { get; } = [];
	public List<Bctex> Textures  { get; } = [];
}