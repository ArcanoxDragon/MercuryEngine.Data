using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Definitions.DataTypes;
using MercuryEngine.Data.Formats;

namespace MercuryEngine.Data.Test;

[TestFixture]
public class MinimapTest
{
	private static readonly Regex MinimapVisRegex = new(@"(\d+)([ @o])", RegexOptions.Compiled);

	[TestCase("Fresh")]
	[TestCase("Hundo")]
	public void TestMinimapVisibility(string profileName)
	{
		var filePath = GetSamusPath(profileName);
		var bmssv = new Bmssv();

		using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
			bmssv.Read(fileStream);

		foreach (var section in bmssv.Sections)
		{
			if (section.Properties.SingleOrDefault(p => p.Key == "MINIMAP_VISIBILITY") is not { Data: minimapGrid_TMinimapVisMap minimapVisibility })
				continue;

			var sortedEntries = minimapVisibility.Entries.OrderByDescending(p => p.Key.Value).Select(p => p.Value.Value).ToList();
			var rawEntryRows = new List<List<string>>();

			foreach (var entry in sortedEntries)
			{
				var rawEntryRow = new List<string>();

				foreach (var match in MinimapVisRegex.Matches(entry).Cast<Match>())
				{
					var count = int.Parse(match.Groups[1].Value);
					var state = match.Groups[2].Value;

					for (var i = 0; i < count; i++)
						rawEntryRow.Add(state);
				}

				rawEntryRows.Add(rawEntryRow);
			}

			var width = rawEntryRows.Max(row => row.Count);
			var height = rawEntryRows.Count;
			using var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			RenderMinimap(bitmap, rawEntryRows);

			var outFileDir = Path.GetDirectoryName(filePath)!;
			var outFilePath = Path.Combine(outFileDir, $"{section.Name}_minimap.png");

			bitmap.Save(outFilePath, ImageFormat.Png);
		}
	}

	private static void RenderMinimap(Bitmap bitmap, IEnumerable<List<string>> rawEntryRows)
	{
		using var graphics = Graphics.FromImage(bitmap);

		foreach (var (y, row) in rawEntryRows.Pairs())
		foreach (var (x, state) in row.Pairs())
		{
			var pen = state switch {
				"@" => Pens.White,
				"o" => Pens.DimGray,
				_   => Pens.Black,
			};

			graphics.DrawRectangle(pen, x, y, 1, 1);
		}
	}

	private static string GetSamusPath(string profileName)
		=> Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSSV", profileName, "samus.bmssv");
}