using System.Text.Json.Serialization;
using MercuryEngine.Data.Core.Framework;
using MercuryEngine.Data.Core.Framework.IO;
using MercuryEngine.Data.Core.Framework.Structures.Fluent;
using MercuryEngine.Data.Types.Pkg;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MercuryEngine.Data.Formats;

public class Pkg : BinaryFormat<Pkg>
{
	internal const int DataSectionStartAlignment = 128;

	[JsonIgnore]
	public override string DisplayName => "PKG";

	public IList<PackageFile> Files => Header.Files;

	#region Private Data

	private PkgHeader Header { get; } = new();

	#endregion

	#region Hooks

	protected override void BeforeWrite(WriteContext context)
	{
		base.BeforeWrite(context);

		if (Files.Count > 0)
		{
			// Adjust the start of the heap section to the correct byte alignment
			var dataStart = Header.Size;
			var misalignment = dataStart % DataSectionStartAlignment;
			var padding = misalignment == 0 ? 0 : DataSectionStartAlignment - misalignment;

			dataStart += padding;
			context.HeapManager.Reset(dataStart);

			// Also correct the header size - it includes all padding after the header, but NOT the "header size" field itself
			Header.HeaderSize = (int) ( dataStart - sizeof(int) );
		}
		else
		{
			// Empty file header size is the size of the "DataSectionSize" field plus the size of the length prefix of the files array
			Header.HeaderSize = sizeof(int) * 2;
		}
	}

	#endregion

	protected override void WriteCore(BinaryWriter writer, WriteContext context)
	{
		base.WriteCore(writer, context);

		// We have to go back and correct the header, since we couldn't know the data section size before writing the file fields
		Header.DataSectionSize = (int) context.HeapManager.TotalAllocated;
		writer.BaseStream.Position = sizeof(int);
		writer.Write(Header.DataSectionSize);
	}

	protected override async Task WriteAsyncCore(AsyncBinaryWriter writer, WriteContext context, CancellationToken cancellationToken = default)
	{
		await base.WriteAsyncCore(writer, context, cancellationToken).ConfigureAwait(false);

		// We have to go back and correct the header, since we couldn't know the data section size before writing the file fields
		var baseStream = await writer.GetBaseStreamAsync(cancellationToken).ConfigureAwait(false);

		Header.DataSectionSize = (int) context.HeapManager.TotalAllocated;
		baseStream.Position = sizeof(int);
		await writer.WriteAsync(Header.DataSectionSize, cancellationToken).ConfigureAwait(false);
	}

	protected override void Describe(DataStructureBuilder<Pkg> builder)
	{
		builder.RawProperty(m => m.Header);
	}
}