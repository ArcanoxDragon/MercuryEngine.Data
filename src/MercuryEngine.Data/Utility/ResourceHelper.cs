using System.Reflection;

namespace MercuryEngine.Data.Utility;

internal static class ResourceHelper
{
	private static readonly Assembly ResourceAssembly = typeof(ResourceHelper).Assembly;
	private static readonly string[] AllResourceNames = ResourceAssembly.GetManifestResourceNames();

	public static Stream OpenResourceFile(string path)
	{
		var resourceId = path.Replace('/', '.').Replace('\\', '.');

		if (AllResourceNames.SingleOrDefault(name => name.EndsWith(resourceId, StringComparison.OrdinalIgnoreCase)) is not { } resourceName)
			throw new FileNotFoundException($"The resource \"{path}\" was not found in the assembly");

		return ResourceAssembly.GetManifestResourceStream(resourceName)!;
	}
}