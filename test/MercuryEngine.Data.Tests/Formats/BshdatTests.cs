using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Extensions;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Bshdat;
using MercuryEngine.Data.Types.Bshdat.CompiledShaders;
using MercuryEngine.Data.Types.Bsmat;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class BshdatTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("bshdat"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadBshdat(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BSHDAT file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var bshdat = new Bshdat();

		try
		{
			bshdat.Read(fileStream);

			foreach (var (i, shaderPair) in bshdat.ShaderProgramPairs.Pairs())
			{
				if (shaderPair is null)
					continue;

				var vertexProgram = shaderPair.ReadCompiledVertexShader();
				var fragmentProgram = shaderPair.ReadCompiledFragmentShader();

				DumpShaderProgramInfo(inFile, relativeTo, i, vertexProgram, ShaderType.Vertex);
				DumpShaderProgramInfo(inFile, relativeTo, i, fragmentProgram, ShaderType.Fragment);
			}
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(bshdat, inFile, relativeTo, print: false);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareBshdat(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading BSHDAT file: {0}", inFile);

		ReadWriteAndCompare<Bshdat>(inFile, relativeTo, quiet: true);
	}

	private static void DumpShaderProgramInfo(string inFile, string relativeTo, int programIndex, CompiledShader shader, ShaderType shaderType)
	{
		var shaderBaseName = Path.GetFileNameWithoutExtension(inFile);

		TestContext.Out.WriteLine($"======== {shaderBaseName} {shaderType} Program ========");

		foreach (var section in shader.Sections)
		{
			if (section is { Type: DataSection.SectionType.Reflection, SectionHeader: ReflectionSectionHeader reflectionHeader })
			{
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Uniform Blocks:");

				var longestUniformBlockName = reflectionHeader.UniformBlocks.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var uniformBlock in reflectionHeader.UniformBlocks)
				{
					var paddedName = uniformBlock.Name.PadLeft(longestUniformBlockName);

					TestContext.Out.WriteLine($"\t{paddedName}: {uniformBlock.VariableCount} variables | Stages: {uniformBlock.Stages} | Bindings: {uniformBlock.Bindings}");
				}

				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("----");
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Uniforms:");

				var longestValueTypeName = reflectionHeader.Uniforms.Select(e => e.ValueType.ToString().Length).MaxOrDefault() + 2; // In case "[]" is suffixed
				var longestUniformName = reflectionHeader.Uniforms.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var uniform in reflectionHeader.Uniforms.OrderBy(u => u.BlockIndex).ThenBy(u => u.BlockOffset))
				{
					var arrayText = uniform.IsArray ? "[]" : string.Empty;
					var valueTypeText = $"{uniform.ValueType}{arrayText}".PadRight(longestValueTypeName);
					var paddedName = uniform.Name.PadLeft(longestUniformName);

					TestContext.Out.WriteLine($"\t[{uniform.BlockIndex,3}/{uniform.BlockOffset,4}] {valueTypeText} {paddedName}: {uniform.Kind} | Stages: {uniform.Stages} | Bindings: {uniform.Bindings}");
				}

				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("----");
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Inputs:");

				longestValueTypeName = reflectionHeader.Inputs.Select(e => e.ValueType.ToString().Length).MaxOrDefault() + 2; // In case "[]" is suffixed

				var longestInputName = reflectionHeader.Inputs.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var input in reflectionHeader.Inputs)
				{
					var arrayText = input.IsArray ? "[]" : string.Empty;
					var valueTypeText = $"{input.ValueType}{arrayText}".PadRight(longestValueTypeName);
					var paddedName = input.Name.PadLeft(longestInputName);

					TestContext.Out.WriteLine($"\t[{input.Location,3}] {valueTypeText} {paddedName} | Stages: {input.Stages}");
				}

				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("----");
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Outputs:");

				longestValueTypeName = reflectionHeader.Outputs.Select(e => e.ValueType.ToString().Length).MaxOrDefault() + 2; // In case "[]" is suffixed

				var longestOutputName = reflectionHeader.Outputs.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var output in reflectionHeader.Outputs)
				{
					var arrayText = output.IsArray ? "[]" : string.Empty;
					var valueTypeText = $"{output.ValueType}{arrayText}".PadRight(longestValueTypeName);
					var paddedName = output.Name.PadLeft(longestOutputName);

					TestContext.Out.WriteLine($"\t[{output.Location,3}/{output.LocationIndex,3}] {valueTypeText} {paddedName} | Stages: {output.Stages}");
				}

				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("----");
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Storage Buffers:");

				var longestStorageBufferName = reflectionHeader.StorageBuffers.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var buffer in reflectionHeader.StorageBuffers)
				{
					var paddedName = buffer.Name.PadLeft(longestStorageBufferName);

					TestContext.Out.WriteLine($"\t{paddedName}: {buffer.VariableCount} variables | Stages: {buffer.Stages} | Bindings: {buffer.Bindings}");
				}

				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("----");
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Buffer Variables:");

				longestValueTypeName = reflectionHeader.BufferVariables.Select(e => e.ValueType.ToString().Length).MaxOrDefault() + 2; // In case "[]" is suffixed

				var longestBufferVariableName = reflectionHeader.BufferVariables.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var variable in reflectionHeader.BufferVariables)
				{
					var arrayText = variable.IsArray ? "[]" : string.Empty;
					var valueTypeText = $"{variable.ValueType}{arrayText}".PadRight(longestValueTypeName);
					var paddedName = variable.Name.PadLeft(longestBufferVariableName);

					TestContext.Out.WriteLine($"\t[{variable.BlockIndex,3}] {valueTypeText} {paddedName} | Stages: {variable.Stages}");
				}

				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("----");
				TestContext.Out.WriteLine();
				TestContext.Out.WriteLine("Varyings:");

				longestValueTypeName = reflectionHeader.Varyings.Select(e => e.ValueType.ToString().Length).MaxOrDefault() + 2; // In case "[]" is suffixed

				var longestVaryingName = reflectionHeader.Varyings.Select(e => e.Name.Length).MaxOrDefault();

				foreach (var varying in reflectionHeader.Varyings)
				{
					var arrayText = varying.IsArray ? "[]" : string.Empty;
					var valueTypeText = $"{varying.ValueType}{arrayText}".PadRight(longestValueTypeName);
					var paddedName = varying.Name.PadLeft(longestVaryingName);

					TestContext.Out.WriteLine($"\t{valueTypeText} {paddedName}");
				}
			}
			else if (section is { Type: DataSection.SectionType.SourceMap, SectionHeader: SourceMapSectionHeader sourceMapHeader })
			{
				if (!Global.WriteOutputFiles)
					continue;

				var relativePath = Path.GetRelativePath(relativeTo, inFile);
				var relativeDirectory = Path.GetDirectoryName(relativePath)!;
				var outFileBaseDir = Path.Join(TestContext.CurrentContext.TestDirectory, "TestFiles", "BSHDAT", relativeDirectory, shaderBaseName, $"program{programIndex}");
				var seenFiles = new HashSet<ShaderSourceFile>();

				Directory.CreateDirectory(outFileBaseDir);

				foreach (var sourceFile in sourceMapHeader.SourceMap.GetOriginalSources())
				{
					if (!seenFiles.Add(sourceFile))
						// Skip it, saw it before
						continue;

					var outFileExtension = ( shaderType, sourceFile.Type ) switch {
						(ShaderType.Vertex, ShaderSourceType.EntryPoint)   => ".vert",
						(ShaderType.Fragment, ShaderSourceType.EntryPoint) => ".frag",
						_                                                  => string.Empty, // Includes have the ".h" extension already
					};
					var outFileName = $"{sourceFile.FileName}{outFileExtension}";
					var outFilePath = Path.Join(outFileBaseDir, outFileName);

					File.WriteAllText(outFilePath, sourceFile.Source);
				}
			}
		}
	}
}