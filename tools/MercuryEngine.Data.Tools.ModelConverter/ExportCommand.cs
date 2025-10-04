using System.CommandLine;
using MercuryEngine.Data.Converters.Bcmdl;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.GameAssets;
using MercuryEngine.Data.Tools.Core;

namespace MercuryEngine.Data.Tools.ModelConverter;

internal class ExportCommand : BaseCommand
{
	public static int Run(ParseResult result)
	{
		// Gather arguments
		var romFsPath = RomFsHelper.GetRomFsPath(result);
		var quiet = result.GetValue(Args.Quiet);
		var inputModelPath = result.GetRequiredValue(Args.Export.InputModelPath);
		var outputModelPath = result.GetRequiredValue(Args.Export.OutputModelPath);
		var outputFormat = result.GetValue(Args.Export.OutputFormat);

		// Get the model to be exported
		var assetResolver = new DefaultGameAssetResolver(romFsPath);
		Bcmdl modelToExport;

		if (Path.IsPathRooted(inputModelPath) && Path.IsPathFullyQualified(inputModelPath))
		{
			// Absolute path - read the file directly
			using var fileStream = File.Open(inputModelPath, FileMode.Open, FileAccess.Read, FileShare.Read);

			modelToExport = new Bcmdl();
			modelToExport.Read(fileStream);
		}
		else
		{
			// Non-absolute path - read the file as an asset
			var modelAsset = assetResolver.GetAsset(inputModelPath);

			if (modelAsset.Location == AssetLocation.None)
			{
				Console.Error.WriteLine($"Input model not found: {modelAsset.RelativePath}");
				return ExitCodes.InputNotFound;
			}

			modelToExport = modelAsset.ReadAs<Bcmdl>();
		}

		// Now export the model
		using var exporter = new GltfExporter(assetResolver);

		if (!quiet)
			exporter.Warning += PrintWarning;

		// Ensure the output directory exists
		if (outputModelPath is { DirectoryName: { } directoryName, Directory: not { Exists: true } })
			Directory.CreateDirectory(directoryName);

		var modelName = Path.GetFileNameWithoutExtension(inputModelPath);

		exporter.LoadBcmdl(modelToExport, modelName);
		exporter.ExportGltf(outputModelPath.FullName, binary: outputFormat == GltfFormat.Binary);

		if (!quiet)
			Console.WriteLine($"Exported model saved to {outputModelPath.FullName}");

		return ExitCodes.Success;
	}
}