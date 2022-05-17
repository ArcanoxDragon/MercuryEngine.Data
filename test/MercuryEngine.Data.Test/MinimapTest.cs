using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using MercuryEngine.Data.Core.Extensions;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.DataTypes.Custom;

namespace MercuryEngine.Data.Test;

[TestFixture]
[SupportedOSPlatform("Windows")]
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

		foreach (var (sectionName, section) in bmssv.Sections)
		{
			if (section.Props?.SingleOrDefault(p => p.Key.Value == "MINIMAP_VISIBILITY") is not { Value.Data: minimapGrid_TMinimapVisMap minimapVisibility })
				continue;

			var parsedMinimap = ParseMinimap(minimapVisibility);
			using var minimapBitmap = RenderMinimap(parsedMinimap);
			var outFileDir = Path.GetDirectoryName(filePath)!;
			var outFilePath = Path.Combine(outFileDir, $"{sectionName}_minimap.png");

			minimapBitmap.Save(outFilePath, ImageFormat.Png);
		}
	}

	private static List<MinimapRow> ParseMinimap(minimapGrid_TMinimapVisMap minimap)
	{
		var sortedMapRows = minimap.Entries.OrderByDescending(p => p.Key.Value).Select(p => p.Value.Value).ToList();
		var parsedRows = new List<MinimapRow>();

		foreach (var row in sortedMapRows)
		{
			var parsedRow = new List<MinimapChunk>();

			foreach (var match in MinimapVisRegex.Matches(row).Cast<Match>())
			{
				var size = int.Parse(match.Groups[1].Value);
				var state = match.Groups[2].Value;
				var chunk = new MinimapChunk(size, (MinimapTileState) state[0]);

				parsedRow.Add(chunk);
			}

			parsedRows.Add(new MinimapRow(parsedRow));
		}

		return parsedRows;
	}

	private static Bitmap RenderMinimap(List<MinimapRow> minimapRows)
	{
		const int TileSize = 4;

		var maxRowLength = minimapRows.Max(row => row.Length);
		var bitmapWidth = TileSize * maxRowLength;
		var bitmapHeight = TileSize * minimapRows.Count;
		var bitmap = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format24bppRgb);
		var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadWrite, bitmap.PixelFormat);
		var byteCount = bitmapData.Height * Math.Abs(bitmapData.Stride);

		unsafe
		{
			var rgbData = new Span<Rgb>((void*) bitmapData.Scan0, byteCount / sizeof(Rgb));

			foreach (var (iRow, row) in minimapRows.Pairs())
			{
				// Render first pixel row, and then we can copy it
				var rowStartY = iRow * TileSize;
				var sourceRowData = rgbData.Slice(rowStartY * bitmapWidth, bitmapWidth);
				var x = 0;

				foreach (var (size, state) in row.Chunks)
				{
					var color = state switch {
						MinimapTileState.Visited => Color.White,
						MinimapTileState.Seen    => Color.DarkGray,
						_                        => Color.Black,
					};
					var rgb = new Rgb(color.R, color.G, color.B);

					// Copy the RGB struct along the row for the size of the chunk
					var chunkSizeInPixels = TileSize * size;
					var chunkSpan = sourceRowData.Slice(x, chunkSizeInPixels);

					chunkSpan.Fill(rgb);
					x += chunkSizeInPixels;
				}

				// Copy row to make it TileSize pixels high
				for (var y = 1; y < TileSize; y++)
				{
					var destRowData = rgbData.Slice(( rowStartY + y ) * bitmapWidth, bitmapWidth);

					sourceRowData.CopyTo(destRowData);
				}
			}
		}

		bitmap.UnlockBits(bitmapData);

		return bitmap;
	}

	private static string GetSamusPath(string profileName)
		=> Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "BMSSV", profileName, "samus.bmssv");

	#region Helper Types

	[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
	private record struct Rgb(byte R, byte G, byte B);

	private record struct MinimapChunk(int Size, MinimapTileState State);

	private sealed record MinimapRow(List<MinimapChunk> Chunks)
	{
		public int Length => Chunks.Sum(c => c.Size);
	}

	private enum MinimapTileState : byte
	{
		Unseen  = (byte) ' ',
		Seen    = (byte) 'o',
		Visited = (byte) '@',
	}

	#endregion
}