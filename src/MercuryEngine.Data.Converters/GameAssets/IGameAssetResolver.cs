using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.Converters.GameAssets;

public interface IGameAssetResolver
{
	bool TryGetAsset(string relativePath, [NotNullWhen(true)] out GameAsset? assetLocation, bool forWriting = false);
	GameAsset GetAsset(string relativePath, bool forWriting = false);
	Bsmat? LoadMaterial(string path);
	Bctex? LoadTexture(string path);
}