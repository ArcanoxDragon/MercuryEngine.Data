using System.CommandLine;
using System.CommandLine.Parsing;

namespace MercuryEngine.Data.Tools.Core;

public static class RomFsHelper
{
	public const string RomFsEnvironmentVariableName = "DREAD_ROMFS";

	public static string GetRomFsPath(ParseResult result)
		=> GetRomFsPath(result.GetValue(CommonArgs.RomFs));

	public static string GetRomFsPath(SymbolResult result)
		=> GetRomFsPath(result.GetValue(CommonArgs.RomFs));

	public static string GetRomFsPath(DirectoryInfo? argument)
	{
		if (argument is { Exists: true })
			return argument.FullName;

		if (Environment.GetEnvironmentVariable(RomFsEnvironmentVariableName) is not { } romfsVariable)
			throw new ApplicationException($"A RomFS path was not provided, and the {RomFsEnvironmentVariableName} environment variable was not set. An extracted Dread RomFS is required.");

		if (!Directory.Exists(romfsVariable))
			throw new DirectoryNotFoundException($"The configured RomFS directory \"{romfsVariable}\" does not exist");

		return romfsVariable;
	}
}