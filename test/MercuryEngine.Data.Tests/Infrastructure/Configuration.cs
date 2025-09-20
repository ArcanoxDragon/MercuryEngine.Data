using Microsoft.Extensions.Configuration;

namespace MercuryEngine.Data.Tests.Infrastructure;

internal static class Configuration
{
	public const string DreadRomFsKey = "DreadRomFs";

	public static IConfigurationRoot Instance { get; }

	public static string RomFsPath
		=> Instance.GetValue<string>(DreadRomFsKey)
		   ?? throw new ApplicationException($"No \"{DreadRomFsKey}\" key was found in the User Secrets!");

	static Configuration()
	{
		ConfigurationBuilder builder = new();

		builder.AddUserSecrets(typeof(Configuration).Assembly);

		Instance = builder.Build();
	}
}