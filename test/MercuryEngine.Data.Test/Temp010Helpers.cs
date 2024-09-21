﻿using MercuryEngine.Data.Types;
using System.Collections.Generic;
using System.Linq;

namespace MercuryEngine.Data.Test;

[TestFixture]
public class Temp010Helpers
{
	private static readonly string[] ComponentDependencyTypes = [
		"FX",
		"StandaloneFX",
		"Collision",
		"Grab",
		"Billboard",
		"SwarmController",
	];

	[TestCaseSource(nameof(ComponentDependencyTypes))]
	public void GenerateComponentTypeCheck(string type)
	{
		List<string> allSubTypes = DreadTypeLibrary.AllTypes
			.Where(t => DreadTypeLibrary.IsChildOf(t.TypeName, $"C{type}Component"))
			.Select(t => t.TypeName)
			.ToList();

		TestContext.Progress.WriteLine("byte Is{0}ComponentType(string typeName) {{", type);

		foreach (var typeName in allSubTypes)
			TestContext.Progress.WriteLine("\tif (typeName == \"{0}\") return TRUE;", typeName);

		TestContext.Progress.WriteLine("\treturn FALSE;");
		TestContext.Progress.WriteLine("}");
	}
}