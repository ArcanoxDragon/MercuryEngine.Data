using System.CommandLine;
using System.CommandLine.Parsing;
using BCnEncoder.Encoder;
using MercuryEngine.Data.Converters.Bcmdl;
using MercuryEngine.Data.TegraTextureLib.Formats;
using MercuryEngine.Data.Tools.Core;

namespace MercuryEngine.Data.Tools.ModelConverter;

internal static class Args
{
	static Args()
	{
		RootCommand.Add(CommonArgs.RomFs);
		RootCommand.Add(Quiet);
		RootCommand.Add(Export.Command);
		RootCommand.Add(Import.Command);
	}

	public static ParseResult Parse(IReadOnlyList<string> args)
		=> RootCommand.Parse(args);

	public static readonly RootCommand RootCommand = new() {
		Description =
			"Converts models between the Dread BCMDL format and the commonly-supported glTF 2.0 format.",
	};

	public static readonly Option<bool> Quiet = new("--quiet", "-q") {
		Recursive = true,
		Description =
			"Prevents warnings and other non-error output from being printed to the console.",
	};

	public static class Export
	{
		static Export()
		{
			Command.Add(InputModelPath);
			Command.Add(OutputModelPath);
			Command.Add(OutputFormat);
			Command.Add(AnimationDirectories);
			Command.Add(AnimationNames);
			Command.Add(AnimationIncludeFilters);
			Command.Add(AnimationExcludeFilters);
			Command.SetAction(ExportCommand.Run);
		}

		public static readonly Command Command = new("export") {
			Description =
				"Exports a BCMDL model (including all materials and textures, by default) to the glTF 2.0 format",
		};

		public static readonly Argument<string> InputModelPath = new("input-model") {
			Description =
				"An absolute or relative path to a BCMDL file to export. Relative paths are resolved " +
				"relative to Dread's RomFS, NOT the current working directory! Relative paths may " +
				"refer to models within Dread PKG files, which will be extracted automatically.",
		};

		public static readonly Option<FileInfo> OutputModelPath = new Option<FileInfo>("--output", "-o") {
			Required = true,
			Description =
				"The file path where the output glTF model will be stored. When not using the \".glb\" " +
				"binary format, images and buffer views associated with the exported model will be " +
				"saved to the same folder as the glTF file itself.",
		}.AcceptLegalFilePathsOnly();

		public static readonly Option<GltfFormat> OutputFormat = new("--format") {
			DefaultValueFactory = result => {
				var outputPathResult = result.GetResult(OutputModelPath);

				if (outputPathResult is null or { Implicit: true })
					return GltfFormat.Binary;

				if (outputPathResult.GetValueOrDefault<FileInfo>() is { } outputModelPath && outputModelPath.Extension.Equals(".gltf", StringComparison.OrdinalIgnoreCase))
					return GltfFormat.Text;

				return GltfFormat.Binary;
			},
			Description =
				"Whether to use the binary \".glb\" format or the text-based \".gltf\" format. " +
				"The binary format allows model data, images, and buffer views to all be included in a single " +
				"file, as opposed to the \".gltf\" format which requires images and buffer views to be saved " +
				"as separate files. Defaults to Binary, unless the output file extension is \".gltf\".",
		};

		#region Animations

		public static readonly Option<List<string>> AnimationDirectories = new Option<List<string>>("--animations-dir") {
			Arity = ArgumentArity.ZeroOrMore,
			Description =
				"A relative path to a directory containing BCSKLA files that will be exported as individual animations. Paths are relative to the " +
				"game's RomFS filesystem, and BCSKLA files will be discovered from both bare RomFS files as well as those in PKG files. This option " +
				"can be specified multiple times to include animations from multiple directories.",
		}.AcceptLegalFilePathsOnly();

		public static readonly Option<List<string>> AnimationNames = new("--animation-name") {
			Arity = ArgumentArity.ZeroOrMore,
			AllowMultipleArgumentsPerToken = true,
			Description =
				"One or more names of animations to include from the provided animations directory. This argument is optional, and mutually " +
				"exclusive with the \"include\" and \"exclude\" filter options. If any animation names are specified, only BCSKA files whose " +
				"filename (excluding the extension) matches one of the provided names will be exported. Multiple animation names can be specified " +
				"by either separating each animation name with a space, or by specifying this argument multiple times.",
			Validators = {
				result => {
					var animationsDirectoryResult = result.GetResult(AnimationDirectories);
					var animationNamesValue = result.GetValueOrDefault<List<string>>();

					if (animationNamesValue is { Count: > 0 } && animationsDirectoryResult is null or { Implicit: true })
						result.AddError($"The \"{AnimationDirectories.Name}\" option must be provided in order to export animations");
				},
			},
		};

		public static readonly Option<List<string>> AnimationIncludeFilters = new("--animations-include") {
			Arity = ArgumentArity.ZeroOrMore,
			Description =
				"A regular expression that can be used to choose which animations are exported. This option can be specified multiple times to " +
				"use multiple filter patterns. Animations will be included if the name of the BCSKLA file (without the extension) matches ANY of " +
				"the specified \"include\" filters. If this option is not provided, all animations in the provided directory will be included.",
			Validators = { ValidateFilterOption },
		};

		public static readonly Option<List<string>> AnimationExcludeFilters = new("--animations-exclude") {
			Arity = ArgumentArity.ZeroOrMore,
			Description =
				"A regular expression that can be used to choose which animations are exported. This option can be specified multiple times to " +
				"use multiple filter patterns. Animations will be excluded if the name of the BCSKLA file (without the extension) matches ANY of " +
				"the specified \"exclude\" filters. Any specified \"exclude\" filters will be applied after any \"include\" filters are applied, " +
				"further refining the set of included animations.",
			Validators = { ValidateFilterOption },
		};

		private static void ValidateFilterOption(OptionResult result)
		{
			var animationsDirectoryResult = result.GetResult(AnimationDirectories);
			var animationNamesResult = result.GetResult(AnimationNames);
			var filtersValue = result.GetValueOrDefault<List<string>>();

			if (filtersValue is { Count: > 0 } && animationsDirectoryResult is null or { Implicit: true })
				result.AddError($"The \"{AnimationDirectories.Name}\" option must be provided in order to export animations");

			if (filtersValue is { Count: > 0 } && animationNamesResult is { Implicit: false })
				result.AddError($"Animation filters cannot be used with the \"{AnimationNames.Name}\" option.");
		}

		#endregion
	}

	public static class Import
	{
		static Import()
		{
			Command.Add(InputModelPath);
			Command.Add(OutputRomFsPath);
			Command.Add(OutputModelName);
			Command.Add(UpdateExistingOutput);
			Command.Add(CompressModelData);
			Command.Add(DefaultShaderName);
			Command.Add(TextureCompressionQuality);
			Command.Add(ParallelTextureEncoding);
			Command.Add(MaxTextureMipCount);
			Command.Add(RelativeModelPath);
			Command.Add(ActorType);
			Command.Add(ActorName);
			Command.SetAction(ImportCommand.Run);
		}

		public static readonly Command Command = new("import") {
			Description =
				"Imports a model in the glTF 2.0 format into Dread's BCMDL format, including creating BSMAT materials and BCTEX textures as necessary.",
		};

		public static readonly Argument<FileInfo> InputModelPath = new Argument<FileInfo>("input-model") {
			Description =
				"A path to a glTF 2.0 \".gltf\" or \".glb\" file to import.",
		}.AcceptExistingOnly();

		public static readonly Option<DirectoryInfo> OutputRomFsPath = new Option<DirectoryInfo>("--output", "-o") {
			Required = true,
			Description =
				"A path to a folder where converted assets will be placed in a structure compatible with Dread's RomFS filesystem. " +
				"The provided folder can be used as the \"romfs\" folder for a LayeredFS/Atmosphere-style mod.",
		}.AcceptLegalFilePathsOnly();

		public static readonly Option<string> OutputModelName = new("--name", "-n") {
			Description =
				"The name to use for the converted model. If not provided, the name of the input glTF file is used.",
			DefaultValueFactory = result => Path.GetFileNameWithoutExtension(result.GetValue(InputModelPath)?.Name) ?? string.Empty,
		};

		public static readonly Option<bool> UpdateExistingOutput = new("--update", "-u") {
			Description =
				"If this option is enabled, the exporter will attempt to find and update an existing \"files.toc\" file within the " +
				"configured output path (instead of always replacing it). Use this option if you are creating a mod with more than " +
				"one custom model, or with other custom assets that must also be listed in \"files.toc\".",
		};

		public static readonly Option<bool> CompressModelData = new("--compress-buffers") {
			Description =
				"Whether or not to compress Vertex and Index buffer data within the exported BCMDL file. Compression is enabled by default.",
			DefaultValueFactory = _ => true,
		};

		public static readonly Option<string?> DefaultShaderName = new("--default-shader") {
			Description =
				"The path to a shader file (BSDHAT) to use for glTF materials that do not explicitly specify a shader. Defaults to " +
				"\"system/shd/mp_opaque_constant_selfilum.bshdat\".",
		};

		#region Textures

		public static readonly Option<CompressionQuality> TextureCompressionQuality = new("--bctex-quality") {
			Description =
				"The compression quality to use when compressing BCTEX textures.",
			DefaultValueFactory = _ => CompressionQuality.Balanced,
		};

		public static readonly Option<bool> ParallelTextureEncoding = new("--bctex-parallel") {
			Description =
				"Whether or not to use multiple threads to encode BCTEX textures faster. Parallel encoding is enabled by default.",
			DefaultValueFactory = _ => true,
		};

		public static readonly Option<int> MaxTextureMipCount = new("--bctex-mip-count") {
			Description =
				"The maximum number of mipmap levels to generate for color-based BCTEX textures.",
			DefaultValueFactory = _ => ImportCommand.DefaultMaxMipCount,
			Validators = {
				result => {
					var value = result.GetValueOrDefault<int>();

					if (value < 1)
						result.AddError("Maximum mip count must be greater than or equal to 1.");
					if (value > XtxTextureInfo.MaxMipCount)
						result.AddError($"Maximum mip count must be less than or equal to {XtxTextureInfo.MaxMipCount}.");
				},
			},
		};

		#endregion

		#region Arbitrary Path Output

		public static readonly Option<string> RelativeModelPath = new("--model-path") {
			Description =
				"A custom path, relative to the output RomFS folder, to use as the base path where the converted model and its " +
				"associated assets are placed.",
			Validators = {
				result => {
					var actorTypeResult = ActorType == null! ? null : result.GetResult(ActorType);
					var modelPathValue = result.GetValueOrDefault<string>();

					if (actorTypeResult is { Implicit: false } && !string.IsNullOrEmpty(modelPathValue))
						result.AddError("A custom model path cannot be provided if an actor type/name are provided");
				},
			},
		};

		#endregion

		#region Actor Output

		public static readonly Option<ActorType> ActorType = new("--actor-type") {
			Description =
				"The type of the actor for which this model is being output. This determines the subfolder of the \"actors\" folder " +
				"to which the model and its assets will be exported.",
			Validators = {
				result => {
					var actorNameResult = ActorName == null! ? null : result.GetResult(ActorName);

					if (actorNameResult is { Implicit: false } && result.Implicit)
						result.AddError("Actor type must be provided if actor name is provided");
				},
			},
		};

		public static readonly Option<string> ActorName = new("--actor-name") {
			Description =
				"The name of the actor for which this model is being output. This determines the subfolder within the actor type folder " +
				"to which the model and its assets will be exported.",
			Validators = {
				result => {
					var actorTypeResult = result.GetResult(ActorType);
					var actorNameValue = result.GetValueOrDefault<string>();

					if (actorTypeResult is { Implicit: false } && string.IsNullOrEmpty(actorNameValue))
						result.AddError("Actor name must be provided if actor type is provided");
				},
			},
		};

		#endregion
	}
}