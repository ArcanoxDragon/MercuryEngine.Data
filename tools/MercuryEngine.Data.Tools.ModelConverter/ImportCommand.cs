using System.CommandLine;
using MercuryEngine.Data.Converters.Bcmdl;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.GameAssets;
using MercuryEngine.Data.Tools.Core;

namespace MercuryEngine.Data.Tools.ModelConverter;

internal class ImportCommand : BaseCommand
{
	public const int DefaultMaxMipCount = 8;

	public static int Run(ParseResult result)
	{
		// Gather arguments
		var romFsPath = RomFsHelper.GetRomFsPath(result);
		var quiet = result.GetValue(Args.Quiet);
		var inputModelPath = result.GetRequiredValue(Args.Import.InputModelPath);
		var outputRomFsPath = result.GetRequiredValue(Args.Import.OutputRomFsPath);
		var outputModelName = result.GetRequiredValue(Args.Import.OutputModelName);
		var updateExistingOutput = result.GetValue(Args.Import.UpdateExistingOutput);
		var compressModelData = result.GetValue(Args.Import.CompressModelData);
		var defaultShaderPath = result.GetValue(Args.Import.DefaultShaderName);
		var textureCompressionQuality = result.GetValue(Args.Import.TextureCompressionQuality);
		var parallelTextureEncoding = result.GetValue(Args.Import.ParallelTextureEncoding);
		var maxTextureMipCount = result.GetValue(Args.Import.MaxTextureMipCount);
		var customRelativeModelPath = result.GetValue(Args.Import.RelativeModelPath);
		var actorType = result.GetValue(Args.Import.ActorType);
		var actorName = result.GetValue(Args.Import.ActorName);

		// Ensure output RomFS directory exists
		outputRomFsPath.Create();

		// Prepare the import
		var assetResolver = new DefaultGameAssetResolver(romFsPath) {
			OutputPath = outputRomFsPath.FullName,
		};
		var importer = new GltfImporter(assetResolver) {
			CompressBuffers = compressModelData,
			TextureEncodingOptions = {
				CompressionQuality = textureCompressionQuality,
				Parallel = parallelTextureEncoding,
				MaxMipLevel = maxTextureMipCount,
			},
		};

		if (!string.IsNullOrEmpty(defaultShaderPath))
			importer.DefaultShader = defaultShaderPath;

		if (!quiet)
			importer.Warning += PrintWarning;

		// Import the model
		GltfImportResult importResult;

		if (!string.IsNullOrEmpty(actorName))
			importResult = importer.ImportGltf(actorType, actorName, outputModelName, inputModelPath.FullName);
		else if (!string.IsNullOrEmpty(customRelativeModelPath))
			importResult = importer.ImportGltf(customRelativeModelPath, outputModelName, inputModelPath.FullName);
		else
			// Ideally shouldn't be able to happen, due to argument validation
			throw new ApplicationException("Unable to determine model output path");

		// Write the result and update the files.toc file
		var filesTocAsset = assetResolver.GetAsset("system/files.toc");
		var filesToc = filesTocAsset.ReadAs<Toc>(useExistingOutput: updateExistingOutput);

		importResult.SaveToRomFs(filesToc);
		filesTocAsset.Write(filesToc);

		if (!quiet)
		{
			Console.WriteLine($"Imported model saved to {importResult.ModelAsset.OutputPath}");

			foreach (var (materialName, (_, materialAsset)) in importResult.Materials)
				Console.WriteLine($"\tMaterial \"{materialName}\" saved to {materialAsset.RelativePath}");

			foreach (var (textureName, (_, textureAsset)) in importResult.Textures)
				Console.WriteLine($"\tTexture \"{textureName}\" saved to {textureAsset.RelativePath}");
		}

		return ExitCodes.Success;
	}
}