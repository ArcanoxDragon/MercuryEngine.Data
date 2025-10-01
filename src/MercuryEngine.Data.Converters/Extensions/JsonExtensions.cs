using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace MercuryEngine.Data.Converters.Extensions;

internal static class JsonExtensions
{
	public static bool TryGetProperty<T>(this JsonNode? node, string propertyName, [NotNullWhen(true)] out T? value)
	{
		value = default;

		if (node is not JsonObject obj)
			return false;

		if (!obj.TryGetPropertyValue(propertyName, out var propertyNode) || propertyNode is null)
			return false;

		try
		{
			value = propertyNode.GetValue<T>();
			return value is not null;
		}
		catch
		{
			return false;
		}
	}
}
