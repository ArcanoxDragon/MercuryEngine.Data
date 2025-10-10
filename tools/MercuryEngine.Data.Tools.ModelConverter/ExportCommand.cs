using System.CommandLine;
using System.Text.RegularExpressions;
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
		var animationDirectories = result.GetValue(Args.Export.AnimationDirectories);

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

		if (animationDirectories is { Count: > 0 })
			FindAndAttachAnimations(result, assetResolver, exporter);

		exporter.ExportGltf(outputModelPath.FullName, binary: outputFormat == GltfFormat.Binary);

		if (!quiet)
			Console.WriteLine($"Exported model saved to {outputModelPath.FullName}");

		return ExitCodes.Success;
	}

	private static void FindAndAttachAnimations(ParseResult result, IGameAssetResolver assetResolver, GltfExporter exporter)
	{
		var animationDirectories = result.GetValue(Args.Export.AnimationDirectories);
		var animationNames = result.GetValue(Args.Export.AnimationNames);
		var animationIncludeFilters = result.GetValue(Args.Export.AnimationIncludeFilters);
		var animationExcludeFilters = result.GetValue(Args.Export.AnimationExcludeFilters);
		var assetPredicate = GetGameAssetPredicate(".bcskla", animationNames, animationIncludeFilters, animationExcludeFilters);

		if (animationDirectories is null)
			return;

		foreach (var asset in FindAnimationAssets(assetResolver, animationDirectories, assetPredicate))
		{
			var name = Path.GetFileNameWithoutExtension(asset.RelativePath);
			var animation = asset.ReadAs<Bcskla>();

			exporter.AttachAnimation(name, animation);
		}
	}

	private static IEnumerable<GameAsset> FindAnimationAssets(IGameAssetResolver assetResolver, List<string> directories, Func<GameAsset, bool> assetPredicate)
		=> directories.SelectMany(directory => assetResolver.EnumerateAssets(directory)).Where(assetPredicate);

	private static Func<GameAsset, bool> GetGameAssetPredicate(string extension, List<string>? names, List<string>? includeFilters, List<string>? excludeFilters)
	{
		if (names is { Count: > 0 })
		{
			var namesSet = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);

			return asset => {
				if (!Path.GetExtension(asset.RelativePath).Equals(extension, StringComparison.OrdinalIgnoreCase))
					return false;

				return namesSet.Contains(Path.GetFileNameWithoutExtension(asset.RelativePath));
			};
		}

		if (includeFilters is null or { Count: 0 } && excludeFilters is null or { Count: 0 })
			// No filters - all assets with the desired extension are included
			return asset => Path.GetExtension(asset.RelativePath).Equals(extension, StringComparison.OrdinalIgnoreCase);

		var includePatterns = includeFilters?.Select(filter => new Regex(filter, RegexOptions.IgnoreCase)).ToList();
		var excludePatterns = excludeFilters?.Select(filter => new Regex(filter, RegexOptions.IgnoreCase)).ToList();

		return asset => {
			if (!Path.GetExtension(asset.RelativePath).Equals(extension, StringComparison.OrdinalIgnoreCase))
				return false;

			var name = Path.GetFileNameWithoutExtension(asset.RelativePath);
			var isIncluded = includePatterns is null or { Count: 0 } || includePatterns.Any(p => p.IsMatch(name));
			var isExcluded = excludePatterns is { Count: > 0 } && excludePatterns.Any(p => p.IsMatch(name));

			return isIncluded && !isExcluded;
		};
	}
}