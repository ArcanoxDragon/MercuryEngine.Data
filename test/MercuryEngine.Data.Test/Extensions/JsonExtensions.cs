using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace MercuryEngine.Data.Test.Extensions;

internal static class JsonExtensions
{
	public static void Sort(this JsonObject obj)
	{
		var properties = obj.ToList();

		foreach (var (propertyName, _) in properties)
			obj.Remove(propertyName);

		properties.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

		foreach (var (propertyName, propertyValue) in properties)
		{
			if (propertyValue is JsonObject childObject)
				childObject.Sort();

			obj.Add(propertyName, propertyValue);
		}
	}
}