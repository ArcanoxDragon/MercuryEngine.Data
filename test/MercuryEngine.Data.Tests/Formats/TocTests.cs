using System.Text;
using MercuryEngine.Data.Definitions.Utility;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Tests.Infrastructure;
using MercuryEngine.Data.Tests.Utility;
using MercuryEngine.Data.Types.Bcmdl;

namespace MercuryEngine.Data.Tests.Formats;

[TestFixture]
public class TocTests : BaseTestFixture
{
	private static IEnumerable<TestCaseData> GetTestFiles()
	{
		foreach (var testCase in GetTestCasesFromRomFs("toc"))
			yield return new TestCaseData(testCase.Arguments[0], RomFsPath) { TestName = testCase.TestName };
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadToc(string inFile, string relativeTo)
		=> TestLoadTocCore(inFile, relativeTo);

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestLoadTocWithExtraStrings(string inFile, string relativeTo)
	{
		PopulateKnownStrings();
		TestLoadTocCore(inFile, relativeTo);
	}

	[TestCaseSource(nameof(GetTestFiles)), Parallelizable]
	public void TestCompareToc(string inFile, string relativeTo)
		=> ReadWriteAndCompare<Toc>(inFile, relativeTo);

	private static void TestLoadTocCore(string inFile, string relativeTo)
	{
		TestContext.Progress.WriteLine("Loading TOC file: {0}", inFile);

		using var fileStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		var toc = new Toc();

		try
		{
			toc.Read(fileStream);
		}
		finally
		{
			try
			{
				DataUtilities.DumpDataStructure(toc, inFile, relativeTo);
			}
			catch (Exception ex)
			{
				TestContext.Error.WriteLine("Error serializing result:");
				TestContext.Error.WriteLine(ex);
			}
		}
	}

	private static void PopulateKnownStrings()
	{
		foreach (var packageFilePath in Directory.EnumerateFiles(RomFsPath, "*.pkg", SearchOption.AllDirectories))
		{
			using var pkgStream = File.Open(packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			foreach (var pkgFile in Pkg.EnumeratePackageFiles(pkgStream))
			{
				if (pkgFile.Length < 4)
					continue;

				try
				{
					using var pkgFileStream = Pkg.OpenPackageFile(pkgStream, pkgFile, keepOpen: true);
					var headerBytes = new byte[4];
					var bytesRead = pkgFileStream.Read(headerBytes.AsSpan());

					if (bytesRead < 4)
						continue;

					pkgFileStream.Position = 0;

					var fourCC = Encoding.ASCII.GetString(headerBytes);

					if (fourCC == "MSUR")
					{
						var bsmat = new Bsmat();

						bsmat.Read(pkgFileStream);

						foreach (var sampler in bsmat.ShaderStages.SelectMany(s => s.Samplers))
						{
							KnownStrings.Record(sampler.TexturePath);
							KnownStrings.Record($"textures/{sampler.TexturePath}");
						}
					}
					else if (fourCC == "MMDL")
					{
						var bcmdl = new Bcmdl();

						bcmdl.Read(pkgFileStream);

						IEnumerable<string?> allModelStrings = [
							..bcmdl.Materials.Where(m => m != null).SelectMany<Material?, string?>(m => [m!.Name, m.Path, m.Prefix, m.Tex1Name, m.Tex2Name, m.Tex3Name]),
							..bcmdl.NodeIds.Where(id => id != null).Select(id => id!.Name),
							..( bcmdl.JointsInfo?.Joints.Where(j => j != null).Select(j => j!.Name) ?? [] ),
						];

						foreach (var str in allModelStrings)
							KnownStrings.Record(str);
					}
				}
				catch
				{
					// Ignore errors - don't care here
				}
			}
		}
	}
}