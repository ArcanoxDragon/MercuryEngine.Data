using System.Text;
using System.Text.RegularExpressions;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Types.Bshdat.CompiledShaders;

public partial class SourceMap(SourceMapSectionHeader sourceMapHeader) : DataStructure<SourceMap>
{
	public ulong Hash { get; private set; }
	public uint  Size { get; private set; }

	public SourceMapEntry[] Entries { get; private set; } = [];

	#region Private Data

	private uint EntryCount { get; set; }

	#endregion

	#region Public Methods

	public IEnumerable<ShaderSourceFile> GetOriginalSources()
	{
		var sourceCodeEntry = Entries.FirstOrDefault(e => e.Type == SourceMapEntryType.SourceMappedAssembly);

		if (sourceCodeEntry is null)
			return [];

		var sourceFiles = new List<ShaderSourceFile>();
		var asmSourceCodeSpan = sourceCodeEntry.Data.AsSpan();

		if (asmSourceCodeSpan[^1] == 0)
			asmSourceCodeSpan = asmSourceCodeSpan[..^1];

		var asmSourceCode = Encoding.UTF8.GetString(asmSourceCodeSpan);

		foreach (var match in ParseSourceMapRegex.EnumerateMatches(asmSourceCode))
		{
			// ValueMatch doesn't include capture information (yet), so we have to manually extract out the data we want to capture
			const string LinePrefix = "#MSDB: (";
			const string SourcePrefix = "_FILE)\"";

			var matchText = asmSourceCode.AsSpan(match.Index, match.Length);
			var matchTextNoPrefix = matchText[LinePrefix.Length..];

			// Find the end of the filename
			var firstColonIndex = matchTextNoPrefix.IndexOf(':');
			var fileNameSpan = matchTextNoPrefix[..firstColonIndex];

			// Find the bounds of the source text itself
			var sourcePrefixIndex = matchTextNoPrefix.IndexOf(SourcePrefix);
			var sourceStart = sourcePrefixIndex + SourcePrefix.Length;
			var sourceEnd = matchTextNoPrefix.LastIndexOf('"');
			var sourceTextSpan = matchTextNoPrefix[sourceStart..sourceEnd];

			// Find out if it was an included file, or the main source text
			var priorToSourcePrefix = matchTextNoPrefix[..sourcePrefixIndex];
			var isInclude = priorToSourcePrefix.EndsWith("INCLUDED");

			// Un-escape the source text
			var sourceTextBuilder = new StringBuilder();

			foreach (var split in UnescapeSourceRegex.EnumerateSplits(sourceTextSpan))
			{
				// Append prior to the split point
				sourceTextBuilder.Append(sourceTextSpan[split]);

				// Make sure we're not at the end of the string, and process the escape character that caused the split
				if (split.End.Value + 1 < sourceTextSpan.Length)
				{
					char escapedCharacter = sourceTextSpan[split.End.Value + 1];

					if (escapedCharacter == 'n')
						sourceTextBuilder.AppendLine();
					else if (escapedCharacter == 't')
						sourceTextBuilder.Append('\t');
					else if (escapedCharacter == '"')
						sourceTextBuilder.Append('"');
					else if (escapedCharacter == '\\')
						sourceTextBuilder.Append('\\');
				}
			}

			sourceFiles.Add(new ShaderSourceFile(isInclude ? ShaderSourceType.Include : ShaderSourceType.EntryPoint, fileNameSpan.ToString(), sourceTextBuilder.ToString()));
		}

		return sourceFiles;
	}

	#endregion

	protected override void ReadCore(BinaryReader reader, ReadContext context)
	{
		base.ReadCore(reader, context);

		Entries = new SourceMapEntry[EntryCount];

		for (var i = 0; i < EntryCount; i++)
		{
			Entries[i] = new SourceMapEntry(sourceMapHeader);
			Entries[i].Read(reader, context);
		}
	}

	protected override async Task ReadAsyncCore(AsyncBinaryReader reader, ReadContext context, CancellationToken cancellationToken)
	{
		await base.ReadAsyncCore(reader, context, cancellationToken).ConfigureAwait(false);

		Entries = new SourceMapEntry[EntryCount];

		for (var i = 0; i < EntryCount; i++)
		{
			Entries[i] = new SourceMapEntry(sourceMapHeader);
			await Entries[i].ReadAsync(reader, context, cancellationToken).ConfigureAwait(false);
		}
	}

	protected override void Describe(DataStructureBuilder<SourceMap> builder)
	{
		builder.Constant(0x65040891, "<magic>");
		builder.Padding(0x14);
		builder.Property(m => m.Hash);
		builder.Padding(0x8);
		builder.Property(m => m.EntryCount);
		builder.Property(m => m.Size);
	}

	#region Regular Expressions

	[GeneratedRegex("""^#MSDB: \((?<FileName>[^:]+):\d+:\d+:(?<FileType>SOURCE_FILE|INCLUDED_FILE)\)"(?<FileText>.*)"\r?$""", RegexOptions.Multiline)]
	private static partial Regex GetParseSourceMapRegex();

	[GeneratedRegex("""\\[rnt"]""")]
	private static partial Regex GetUnescapeSourceRegex();

	private static readonly Regex ParseSourceMapRegex = GetParseSourceMapRegex();
	private static readonly Regex UnescapeSourceRegex = GetUnescapeSourceRegex();

	#endregion
}