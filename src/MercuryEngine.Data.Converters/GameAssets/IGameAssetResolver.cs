using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.Converters.GameAssets;

public interface IGameAssetResolver
{
	bool TryGetAssetLocation(string relativePath, [NotNullWhen(true)] out AssetLocation? assetLocation, bool forWriting = false);
	AssetLocation GetAssetLocation(string relativePath, bool forWriting = false);
	Bsmat? LoadMaterial(string path);
	Bctex? LoadTexture(string path);
}