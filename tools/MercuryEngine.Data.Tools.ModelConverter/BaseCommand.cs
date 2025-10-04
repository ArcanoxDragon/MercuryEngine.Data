using Pastel;

namespace MercuryEngine.Data.Tools.ModelConverter;

internal abstract class BaseCommand
{
	protected static void PrintWarning(string warning)
	{
		string fullMessage = $"{"WARNING:".PastelBg(ConsoleColor.Yellow)} {warning.Pastel(ConsoleColor.Yellow)}";

		Console.Error.WriteLine(fullMessage);
	}
}