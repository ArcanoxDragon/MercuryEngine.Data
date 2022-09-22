using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MercuryEngine.Data.Test.Extensions;

internal static class JsonExtensions
{
	public static void Sort(this JObject obj)
	{
		var properties = obj.Properties().ToList();

		foreach (var property in properties)
			property.Remove();

		properties.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

		foreach (var property in properties)
		{
			obj.Add(property);

			if (property.Value is JObject childObject)
				childObject.Sort();
		}
	}
}