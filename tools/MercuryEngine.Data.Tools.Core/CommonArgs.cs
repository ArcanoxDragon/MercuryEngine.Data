using System.CommandLine;

namespace MercuryEngine.Data.Tools.Core;

public static class CommonArgs
{
	public static readonly Option<DirectoryInfo> RomFs = new Option<DirectoryInfo>("--romfs") {
		Recursive = true,
		Description =
			"An absolute path to a folder containing extracted Dread RomFS files. " +
			"This can also be specified via a \"DREAD_ROMFS\" environment variable. When " +
			"this option and the environment variable are both present, this option will " +
			"take priority.",
	}.AcceptExistingOnly();
}