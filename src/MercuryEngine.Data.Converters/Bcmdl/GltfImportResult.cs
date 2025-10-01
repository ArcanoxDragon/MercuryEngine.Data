using MercuryEngine.Data.Converters.GameAssets;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.TegraTextureLib.Formats;

namespace MercuryEngine.Data.Converters.Bcmdl;

public class GltfImportResult(ActorType actorType, string actorName, string modelName, Formats.Bcmdl model, GameAsset modelAsset)
{
	public ActorType     ActorType  { get; } = actorType;
	public string        ActorName  { get; } = actorName;
	public string        ModelName  { get; } = modelName;
	public Formats.Bcmdl Model      { get; } = model;
	public GameAsset     ModelAsset { get; } = modelAsset;

	public Dictionary<string, (Bsmat, GameAsset)> Materials { get; } = [];
	public Dictionary<string, (Bctex, GameAsset)> Textures  { get; } = [];

	public void SaveToRomFs(Toc? filesToc = null)
	{
		uint assetSize;

		// Write textures
		foreach (var (bctex, bctexAsset) in Textures.Values)
		{
			assetSize = bctexAsset.Write(bctex);
			filesToc?.PutFileSize(bctexAsset.AssetId, assetSize);
		}

		// Write materials
		foreach (var (bsmat, bsmatAsset) in Materials.Values)
		{
			assetSize = bsmatAsset.Write(bsmat);
			filesToc?.PutFileSize(bsmatAsset.AssetId, assetSize);
		}

		// Write the model itself
		assetSize = ModelAsset.Write(Model);
		filesToc?.PutFileSize(ModelAsset.AssetId, assetSize);
	}
}