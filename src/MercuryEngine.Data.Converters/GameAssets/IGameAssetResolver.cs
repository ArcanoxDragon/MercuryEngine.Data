using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.Converters.GameAssets;

public interface IGameAssetResolver
{
	// TODO: Documentation

	bool TryGetAsset(string relativePath, [NotNullWhen(true)] out GameAsset? assetLocation);
	bool TryGetAsset(string relativePath, AssetAccessMode accessMode, [NotNullWhen(true)] out GameAsset? assetLocation);
	GameAsset GetAsset(string relativePath, AssetAccessMode accessMode = AssetAccessMode.Read);
	Bsmat? LoadMaterial(string path);
	Bctex? LoadTexture(string path);
}